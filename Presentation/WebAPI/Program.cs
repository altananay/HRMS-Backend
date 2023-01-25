using Application;
using Application.Extensions;
using Application.Utilities.IoC;
using Application.Utilities.JWT;
using Application.Utilities.Security.Encryption;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new AutofacServiceRegistration()));


builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddCors();

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
            IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
        };
    });

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

app.UseStaticFiles();

app.UseCors(builder => builder.WithOrigins("http://localhost:3000", "https://localhost:3000").AllowAnyHeader().AllowAnyMethod());

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();