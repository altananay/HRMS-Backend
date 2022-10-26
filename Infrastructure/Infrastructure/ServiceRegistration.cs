using Application.Abstractions;
using Application.Utilities.Helpers;
using Infrastructure.Services.JWT;
using Infrastructure.Services.Mernis;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITokenHelper, TokenHandler>();
            serviceCollection.AddScoped<ICheckPersonService, CheckPerson>();
        }
    }
}