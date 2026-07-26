using Application.Abstractions;
using Application.Abstractions.Storage;
using Domain.Enums;
using Infrastructure.Services.Identity;
using Infrastructure.Services.JWT;
using Infrastructure.Services.Security;
using Infrastructure.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddStorage(configuration);

            services.AddIdentityVerification(configuration);

            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

            services.AddScoped<IUserSecurityStateProvider, CachedUserSecurityStateProvider>();

            return services;
        }

        private static void AddIdentityVerification(this IServiceCollection services, IConfiguration configuration)
        {
            var useMernis = string.Equals(
                configuration["IdentityVerification:Provider"], "Mernis", StringComparison.OrdinalIgnoreCase);

            if (useMernis)
            {
                services.AddSingleton<IIdentityVerificationService, MernisIdentityVerificationService>();
            }
            else
            {
                services.AddSingleton<IIdentityVerificationService, NullIdentityVerificationService>();
            }
        }

        private static void AddStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = Enum.TryParse<StorageProvider>(
                configuration["Storage:Provider"], ignoreCase: true, out var parsed)
                ? parsed
                : StorageProvider.Local;

            if (provider == StorageProvider.R2)
            {
                services.AddOptions<R2Options>()
                    .Bind(configuration.GetSection(R2Options.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                services.AddSingleton<IStorage, R2Storage>();
            }
            else
            {
                services.AddSingleton<IStorage, LocalStorage>();
            }

            services.AddSingleton<IStorageService, StorageService>();
        }
    }
}
