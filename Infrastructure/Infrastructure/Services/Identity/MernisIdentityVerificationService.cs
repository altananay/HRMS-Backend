using System.ServiceModel;
using Application.Abstractions;
using MernisServiceReference;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Identity
{
    public sealed class MernisIdentityVerificationService : IIdentityVerificationService
    {
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
                            request.FirstName.ToUpperInvariant(),
                            request.LastName.ToUpperInvariant(),
                            request.BirthYear)))
                    .WaitAsync(CallTimeout, cancellationToken);

                return response.Body.TCKimlikNoDogrulaResult;
            }
            catch (Exception exception) when (
                exception is CommunicationException or TimeoutException or OperationCanceledException)
            {
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
