namespace Application.Abstractions
{
    public sealed record IdentityVerificationRequest(
        string NationalId,
        string FirstName,
        string LastName,
        int BirthYear);

    public interface IIdentityVerificationService
    {
        Task<bool> VerifyAsync(IdentityVerificationRequest request, CancellationToken cancellationToken = default);
    }
}
