using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Application;
using Application.Abstractions;
using Application.Utilities.JWT;
using Application.Utilities.Security.Encryption;
using Infrastructure;
using Infrastructure.Services.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Persistence;
using Persistence.Seeding;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using WebAPI.Infrastructure;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------
// Configuration â€” fail fast rather than NullReferenceException at first use.
// ---------------------------------------------------------------------------------------------
builder.Services
    .AddOptions<TokenOptions>()
    .Bind(builder.Configuration.GetSection("TokenOptions"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "TokenOptions:Issuer is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "TokenOptions:Audience is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.SecurityKey), "TokenOptions:SecurityKey is required.")
    .Validate(options => (options.SecurityKey?.Length ?? 0) >= 32,
        "TokenOptions:SecurityKey must be at least 32 characters.")
    .ValidateOnStart();

var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()
    ?? throw new InvalidOperationException(
        "TokenOptions section is missing. See appsettings.json for the expected shape; supply the " +
        "signing key via user-secrets in Development or TokenOptions__SecurityKey in Production.");

// ---------------------------------------------------------------------------------------------
// Logging â€” Console always, Seq when configured. The MongoDB sink is gone: it wrote domain data
// and logs into the same database, and the capped `logs` collection is replaced by Seq.
// ---------------------------------------------------------------------------------------------
var seqUrl = builder.Configuration["Serilog:Seq:ServerUrl"];

var loggerConfiguration = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console();

if (!string.IsNullOrWhiteSpace(seqUrl))
{
    loggerConfiguration = loggerConfiguration.WriteTo.Seq(seqUrl);
}

builder.Host.UseSerilog(loggerConfiguration.CreateLogger());

// ---------------------------------------------------------------------------------------------
// Services â€” built-in DI only. Autofac, Castle DynamicProxy and the ServiceTool locator are gone;
// cross-cutting concerns are MediatR pipeline behaviors registered in AddApplicationServices.
// ---------------------------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Application.Abstractions.ICurrentUserService, CurrentUserService>();

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddMemoryCache();

// ---------------------------------------------------------------------------------------------
// CORS â€” the previous policy chained .WithOrigins(...) and then .AllowAnyOrigin(), and the
// wildcard won, so the named origin list was decorative and the policy was effectively open.
// ---------------------------------------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"));
});

// ---------------------------------------------------------------------------------------------
// Controllers. The global ValidationFilter and FluentValidation auto-validation are both gone â€”
// ValidationBehavior in the MediatR pipeline is now the single validation stack, so the default
// model-state 400 is wanted again and SuppressModelStateInvalidFilter is no longer set.
// ---------------------------------------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as their names, not their ordinals. JobApplicationStatus, JobType,
        // UserType and StorageProvider all cross the wire; as numbers they are unreadable to a
        // client and, worse, silently change meaning if a member is ever inserted mid-enum — the
        // same reason they are stored as text in PostgreSQL.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------------------------------------------------------------------------------------------
// Authentication / authorization.
// ---------------------------------------------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,

            // Tokens are minted and validated by this same service, so there is no reason to
            // tolerate clock drift â€” and zero skew makes expiry assertions deterministic in tests.
            ClockSkew = TimeSpan.Zero
        };

        // Signature and lifetime are not enough. A JWT is self-validating, so without this hook an
        // access token stays valid until it expires no matter what happens server-side â€” meaning
        // "log out everywhere", a password change, an account deactivation, a role revocation and
        // even refresh-token theft detection all silently do nothing for up to fifteen minutes.
        //
        // The security_stamp claim was already being written by JwtTokenService; nothing read it.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;

                // Reject any token minted against a previous claim layout.
                if (principal?.FindFirstValue(JwtTokenService.TokenSchemaVersionClaim)
                    != JwtTokenService.CurrentTokenSchemaVersion)
                {
                    context.Fail("Token was issued against an unsupported claim schema.");
                    return;
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var tokenStamp = principal.FindFirstValue("security_stamp");

                if (!Guid.TryParse(userId, out var id) || !Guid.TryParse(tokenStamp, out var stamp))
                {
                    context.Fail("Token is missing the subject or security stamp claim.");
                    return;
                }

                var provider = context.HttpContext.RequestServices
                    .GetRequiredService<IUserSecurityStateProvider>();

                var state = await provider.GetAsync(id, context.HttpContext.RequestAborted);

                if (state is null || !state.IsActive || state.SecurityStamp != stamp)
                {
                    // Same failure for a deleted user, a disabled account and a rotated stamp â€” the
                    // client only needs to know the session is over.
                    context.Fail("The session is no longer valid.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // THE structural fix. There is not a single [Authorize] attribute in the pre-migration
    // codebase; authorization lived only in [SecuredOperation] aspects on manager methods, and
    // those were commented out on most write paths. A fallback policy inverts the default from
    // "open unless someone remembered an aspect" to "closed unless someone opted out", so a new
    // endpoint is protected by default rather than by diligence.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ---------------------------------------------------------------------------------------------
// Rate limiting â€” the only real defence against credential stuffing on the auth endpoints.
// ---------------------------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5)
            }));
});

// ---------------------------------------------------------------------------------------------
// Swagger â€” with a Bearer definition, which the previous setup lacked entirely, making it
// impossible to exercise an authenticated endpoint from the Swagger UI.
// ---------------------------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HRMS API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token only â€” Swagger adds the \"Bearer \" prefix."
    };

    options.AddSecurityDefinition("Bearer", scheme);

    // Swashbuckle 10 / Microsoft.OpenApi 2.x takes a factory so the requirement can resolve its
    // scheme reference against the document being generated.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// Development/Testing only. In Production migrations are applied deliberately â€” by a migration
// bundle or an init container â€” not as a side effect of a web process starting up, where two
// instances booting at once would race each other.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<HrmsDbContext>().Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(app.Services);
}

// ---------------------------------------------------------------------------------------------
// Pipeline.
// ---------------------------------------------------------------------------------------------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("ApiCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Enrich logs with the caller's identity. The previous version of this middleware guarded with
// `context.User?.Identity?.IsAuthenticated != null || true`, which is unconditionally true, and
// discarded the IDisposable returned by PushProperty so the properties were never popped off the
// ambient AsyncLocal stack. Both are fixed here.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        using (LogContext.PushProperty("UserId", context.User.FindFirstValue(ClaimTypes.NameIdentifier)))
        using (LogContext.PushProperty("UserName", context.User.Identity.Name))
        {
            await next(context);
        }
    }
    else
    {
        await next(context);
    }
});

app.MapControllers();

app.Run();

// Required for WebApplicationFactory<Program> in the functional test project: top-level statements
// generate an internal Program class, which the factory's generic constraint cannot bind to.
public partial class Program { }
