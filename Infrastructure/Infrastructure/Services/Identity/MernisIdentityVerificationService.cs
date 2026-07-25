using System.ServiceModel;
using Application.Abstractions;
using MernisServiceReference;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Identity
{
    /// <summary>
    /// Verifies a Turkish national identity against the Mernis (KPS) SOAP service.
    /// </summary>
    /// <remarks>
    /// Enabled by <c>IdentityVerification:Provider=Mernis</c>; the default remains the fail-closed
    /// null implementation, so tests and local development never call a government endpoint.
    ///
    /// Its predecessor, <c>CheckPerson</c>, had the entire body <c>return true</c> — every caller
    /// believed identity had been checked while nothing had been. This one actually calls the
    /// service, and any failure is reported as unverified rather than swallowed into a pass.
    /// </remarks>
    public sealed class MernisIdentityVerificationService : IIdentityVerificationService
    {
        /// <summary>
        /// Bounds how long a registration can hang waiting on an external government service.
        /// </summary>
        /// <remarks>
        /// The original client had no timeout at all, so an unresponsive endpoint would hold the
        /// request thread for the WCF default of a minute.
        /// </remarks>
        private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(10);

        private readonly ILogger<MernisIdentityVerificationService> _logger;

        public MernisIdentityVerificationService(ILogger<MernisIdentityVerificationService> logger)
            => _logger = logger;

        public async Task<bool> VerifyAsync(
            IdentityVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(request.NationalId, out var nationalId))
            {
                // Not a transport failure — the input simply cannot be a TCKN.
                return false;
            }

            KPSPublicSoapClient? client = null;

            try
            {
                client = new KPSPublicSoapClient(KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
                client.Endpoint.Binding.SendTimeout = CallTimeout;
                client.Endpoint.Binding.ReceiveTimeout = CallTimeout;

                var response = await client.TCKimlikNoDogrulaAsync(
                    new TCKimlikNoDogrulaRequest(
                        new TCKimlikNoDogrulaRequestBody(
                            nationalId,
                            // The service matches on uppercase Turkish forms.
                            request.FirstName.ToUpperInvariant(),
                            request.LastName.ToUpperInvariant(),
                            request.BirthYear)))
                    .WaitAsync(CallTimeout, cancellationToken);

                return response.Body.TCKimlikNoDogrulaResult;
            }
            catch (Exception exception) when (
                exception is CommunicationException or TimeoutException or OperationCanceledException)
            {
                // Fails closed. A verification service that cannot reach its authority has not
                // verified anything, and must not report success — which is the exact mistake the
                // previous `return true` stub made permanent. Never log the request: it carries a
                // national ID, full name and birth year.
                _logger.LogWarning(exception, "Mernis verification could not be completed; treating as unverified.");
                return false;
            }
            finally
            {
                if (client is not null)
                {
                    await CloseQuietlyAsync(client);
                }
            }
        }

        /// <remarks>
        /// WCF clients throw from <c>CloseAsync</c> when the channel is already faulted, so a failed
        /// call would otherwise mask its own cause with a cleanup exception.
        /// </remarks>
        private static async Task CloseQuietlyAsync(KPSPublicSoapClient client)
        {
            try
            {
                await client.CloseAsync();
            }
            catch
            {
                client.Abort();
            }
        }
    }
}
