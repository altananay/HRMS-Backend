using Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Identity
{
    /// <summary>
    /// Identity verification that is switched off, and says so.
    /// </summary>
    /// <remarks>
    /// The default in Development and in tests, so the suite never calls a government SOAP endpoint.
    ///
    /// It <b>fails closed</b> — returning <c>false</c> rather than <c>true</c>. That is the whole
    /// point: the previous <c>CheckPerson</c> implementation was
    /// <c>bool CheckPerson() { return true; }</c>, an unconditional pass that made every caller
    /// believe identity had been verified when nothing had been checked at all. A verification
    /// service that cannot verify must not report success.
    ///
    /// Callers decide what to do with a negative result; registration treats verification as
    /// required only when <c>IdentityVerification:Provider</c> is set to Mernis.
    /// </remarks>
    public sealed class NullIdentityVerificationService : IIdentityVerificationService
    {
        private readonly ILogger<NullIdentityVerificationService> _logger;

        public NullIdentityVerificationService(ILogger<NullIdentityVerificationService> logger) => _logger = logger;

        public Task<bool> VerifyAsync(
            IdentityVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            // Never log the request: it carries a national ID plus full name and birth year.
            _logger.LogDebug("Identity verification is disabled; reporting unverified.");

            return Task.FromResult(false);
        }
    }
}
