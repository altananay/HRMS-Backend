using Application.Abstractions;
using Application.Abstractions.Storage;
using Application.Utilities.Helpers;
using Infrastructure.Services.JWT;
using Infrastructure.Services.Mernis;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Storage.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITokenHelper, TokenHandler>();
            serviceCollection.AddScoped<ICheckPersonService, CheckPerson>();
            serviceCollection.AddScoped<IStorage, AzureStorage>();
            serviceCollection.AddScoped<IStorageService, StorageService>();
        }
    }
}