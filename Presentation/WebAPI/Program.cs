using Application;
using Application.Extensions;
using Application.Utilities.IoC;
using Application.Utilities.JWT;
using Application.Utilities.Security.Encryption;
using Application.Validators.JobAdvertisements;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new AutofacServiceRegistration()));


builder.Services.AddMediatR(mr => mr.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
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
            NameClaimType = ClaimTypes.Name, //Jwt üzerinde name claimine karþýlýk gelen deðeri
                                             //User.identity.name propertysinden al.
                                             //todo: Loglama için
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
    .MinimumLevel.Information().CreateLogger();

builder.Host.UseSerilog(log);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddDependencyResolvers(new ICoreModule[] { new CoreModule() });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "The API", Version = "v1" });
});

//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());


app.UseStaticFiles();

app.Use(async (ctx, next) =>
{
    using (LogContext.PushProperty("IpAddress", ctx.Connection.RemoteIpAddress))
    {
        await next(ctx);
    }
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ApiCorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();