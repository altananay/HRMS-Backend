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

            // Stateless and therefore safe as a singleton — unlike the scoped TokenHandler it
            // replaces, which held the expiry in a mutable field while being injected into
            // singleton managers.
            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

            // Scoped: wraps IUserRepository, which wraps the DbContext.
            services.AddScoped<IUserSecurityStateProvider, CachedUserSecurityStateProvider>();

            return services;
        }

        /// <remarks>
        /// Off by default. Both implementations fail closed, so the difference is only whether the
        /// government service is actually consulted — never whether an unverified identity can pass
        /// as verified, which is what the old <c>return true</c> stub allowed.
        /// </remarks>
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

        /// <remarks>
        /// Storage is selected by configuration rather than hard-wired. It used to be
        /// <c>AddScoped&lt;IStorage, AzureStorage&gt;()</c> with no switch, so local development and
        /// any test touching uploads needed real cloud credentials.
        /// </remarks>
        private static void AddStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = Enum.TryParse<StorageProvider>(
                configuration["Storage:Provider"], ignoreCase: true, out var parsed)
                ? parsed
                : StorageProvider.Local;

            if (provider == StorageProvider.R2)
            {
                // Validated only when R2 is actually selected, so a Local setup never needs R2
                // credentials present — but an R2 setup fails at startup rather than on first upload.
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
