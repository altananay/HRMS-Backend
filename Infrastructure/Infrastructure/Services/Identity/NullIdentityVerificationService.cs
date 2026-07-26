using Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Identity
{
    public sealed class NullIdentityVerificationService : IIdentityVerificationService
    {
        private readonly ILogger<NullIdentityVerificationService> _logger;

        public NullIdentityVerificationService(ILogger<NullIdentityVerificationService> logger) => _logger = logger;

        public Task<bool> VerifyAsync(
            IdentityVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Identity verification is disabled; reporting unverified.");

            return Task.FromResult(false);
        }
    }
}
