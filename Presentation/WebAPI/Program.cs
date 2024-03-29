using Application;
using Application.CrossCuttingConcerns.Validation.Validators.JobAdvertisements;
using Application.Utilities.Extensions;
using Application.Utilities.IoC;
using Application.Utilities.JWT;
using Application.Utilities.Security.Encryption;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new AutofacServiceRegistration()));


builder.Services.AddMediatR(mr => mr.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder => {
    builder.WithOrigins("http://localhost:3000", "https://localhost:3000", "https://hrmstez.netlify.app").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
}));
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>()).AddFluentValidation(configuration => 
    configuration.RegisterValidatorsFromAssemblyContaining<CreateJobAdvertisementValidator>().RegisterValidatorsFromAssemblyContaining<UpdateJobAdvertisementValidator>()).ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);


IConfiguration configuration = builder.Configuration;

var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey),
            NameClaimType = ClaimTypes.Name,
        };
    });

var connectionString = builder.Configuration.GetConnectionString("MongoDb");


Logger log = new LoggerConfiguration().WriteTo.Seq(builder.Configuration["Seq:SeqUrl"]).WriteTo.MongoDBBson(conf =>
    {
        var mongoDbInstance = new MongoClient(connectionString).GetDatabase("humanresource");
        conf.SetMongoDatabase(mongoDbInstance);
        conf.SetCollectionName("logs");
        conf.SetCreateCappedCollection(300);
        conf.SetBatchPeriod(TimeSpan.FromSeconds(0.1));
    }).Enrich.FromLogContext()
    .MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error).CreateLogger();

builder.Host.UseSerilog(log);

builder.Services.AddDependencyResolvers(new ICoreModule[] { new CoreModule() });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "The API", Version = "v1" });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (ctx, next) =>
{
    using (LogContext.PushProperty("IpAddress", ctx.Connection.RemoteIpAddress))
    {
        await next(ctx);
    }
});

app.ConfigureExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ApiCorsPolicy");

app.UseAuthentication();

app.UseAuthorization();
app.Use(async (context, next) =>
{
    var username = context.User?.Identity?.IsAuthenticated != null || true ? context.User.Identity.Name : null;
    var id = context.User?.Identity?.IsAuthenticated != null || true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
    var roles = context.User?.Identity?.IsAuthenticated != null || true ? context.User.FindFirst(ClaimTypes.Role)?.Value : null;
    LogContext.PushProperty("Username", username);
    LogContext.PushProperty("Id", id);
    LogContext.PushProperty("Roles", roles);
    await next();
});

app.MapControllers();

app.Run();