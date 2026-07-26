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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Application.Abstractions.ICurrentUserService, CurrentUserService>();

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddMemoryCache();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"));
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;

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
                    context.Fail("The session is no longer valid.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var authPermitLimit = builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
var authWindowMinutes = builder.Configuration.GetValue("RateLimiting:Auth:WindowMinutes", 5);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authPermitLimit,
                Window = TimeSpan.FromMinutes(authWindowMinutes)
            }));
});

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

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<HrmsDbContext>().Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(app.Services);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// No UseStaticFiles: nothing is served from disk, and adding it would expose the web root.
// No UseSerilogRequestLogging: it logs through the static Log.Logger, which this app never sets.
app.UseRouting();
app.UseCors("ApiCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

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

public partial class Program { }
