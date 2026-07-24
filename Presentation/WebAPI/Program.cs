using System.Security.Claims;
using System.Threading.RateLimiting;
using Application;
using Application.Utilities.JWT;
using Application.Utilities.Security.Encryption;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using WebAPI.Infrastructure;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------
// Configuration — fail fast rather than NullReferenceException at first use.
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
// Logging — Console always, Seq when configured. The MongoDB sink is gone: it wrote domain data
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
// Services — built-in DI only. Autofac, Castle DynamicProxy and the ServiceTool locator are gone;
// cross-cutting concerns are MediatR pipeline behaviors registered in AddApplicationServices.
// ---------------------------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Application.Abstractions.ICurrentUserService, CurrentUserService>();

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();

builder.Services.AddMemoryCache();

// ---------------------------------------------------------------------------------------------
// CORS — the previous policy chained .WithOrigins(...) and then .AllowAnyOrigin(), and the
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
// Controllers. The global ValidationFilter and FluentValidation auto-validation are both gone —
// ValidationBehavior in the MediatR pipeline is now the single validation stack, so the default
// model-state 400 is wanted again and SuppressModelStateInvalidFilter is no longer set.
// ---------------------------------------------------------------------------------------------
builder.Services.AddControllers();
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
            // tolerate clock drift — and zero skew makes expiry assertions deterministic in tests.
            ClockSkew = TimeSpan.Zero
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
// Rate limiting — the only real defence against credential stuffing on the auth endpoints.
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
// Swagger — with a Bearer definition, which the previous setup lacked entirely, making it
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
        Description = "Paste the access token only — Swagger adds the \"Bearer \" prefix."
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
