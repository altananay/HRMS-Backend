namespace Application.Abstractions
{
    /// <summary>Identity to verify against the national registry.</summary>
    public sealed record IdentityVerificationRequest(
        string NationalId,
        string FirstName,
        string LastName,
        int BirthYear);

    /// <summary>
    /// Verifies a Turkish national identity (Mernis / KPS).
    /// </summary>
    /// <remarks>
    /// Replaces <c>ICheckPersonService</c>, whose implementation was
    /// <c>bool CheckPerson() => true;</c> — a parameterless stub that unconditionally reported
    /// success, so registration performed no identity check whatsoever despite five
    /// System.ServiceModel packages being referenced for it.
    ///
    /// The interface takes the data it actually needs and returns a real result. Phase 5 supplies
    /// two implementations: a null one that <b>fails closed</b> (the default in Development and
    /// tests, so the suite never calls a government SOAP endpoint) and the real client behind a
    /// configuration flag.
    /// </remarks>
    public interface IIdentityVerificationService
    {
        Task<bool> VerifyAsync(IdentityVerificationRequest request, CancellationToken cancellationToken = default);
    }
}
