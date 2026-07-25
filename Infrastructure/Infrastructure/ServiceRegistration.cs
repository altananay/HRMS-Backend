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
            // Storage is selectable rather than hard-wired. It used to be
            // `AddScoped<IStorage, AzureStorage>()` with no switch, so local development and any
            // functional test touching uploads needed a real Azure connection string.
            var provider = Enum.TryParse<StorageProvider>(
                configuration["Storage:Provider"], ignoreCase: true, out var parsed)
                ? parsed
                : StorageProvider.Local;

            if (provider == StorageProvider.Azure)
            {
                services.AddSingleton<IStorage, AzureStorage>();
            }
            else
            {
                services.AddSingleton<IStorage, LocalStorage>();
            }

            services.AddSingleton<IStorageService, StorageService>();

            // Phase 5 adds the real Mernis SOAP client behind IdentityVerification:Provider=Mernis.
            // Until then the null implementation is registered, and it fails closed.
            services.AddSingleton<IIdentityVerificationService, NullIdentityVerificationService>();

            // Stateless and therefore safe as a singleton â€” unlike the scoped TokenHandler it
            // replaces, which held the expiry in a mutable field while being injected into
            // singleton managers.
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

            // Scoped: wraps IUserRepository, which wraps the DbContext.
            services.AddScoped<IUserSecurityStateProvider, CachedUserSecurityStateProvider>();

            return services;
        }
    }
}
