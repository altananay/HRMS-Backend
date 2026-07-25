namespace Application.Abstractions
{
    public enum PasswordVerificationOutcome
    {
        Failed = 0,
        Success = 1,

        /// <summary>
        /// The password is correct but was hashed with outdated parameters.
        /// </summary>
        /// <remarks>
        /// Lets the login path transparently re-hash with current settings, so raising the work
        /// factor in future upgrades every user on their next sign-in instead of requiring a reset.
        /// </remarks>
        SuccessRehashNeeded = 2
    }

    /// <summary>
    /// Hashes and verifies passwords.
    /// </summary>
    /// <remarks>
    /// Replaces <c>HashingHelper</c>, which was not a password KDF at all: a single round of
    /// HMACSHA512 using <c>hmac.Key</c> as the salt, with no work factor. Its verify loop also
    /// returned on the first mismatching byte — a timing oracle — and threw
    /// <c>IndexOutOfRangeException</c> if the stored hash was shorter than 64 bytes.
    /// </remarks>
    public interface IPasswordHasher
    {
        string Hash(string password);

        PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword);
    }
}
