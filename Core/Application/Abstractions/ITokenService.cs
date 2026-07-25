using Domain.Entities;

namespace Application.Abstractions
{
    /// <summary>An issued access token and its expiry.</summary>
    public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

    /// <summary>
    /// Mints access tokens.
    /// </summary>
    /// <remarks>
    /// One method, replacing <c>CreateToken(JobSeeker)</c>, <c>CreateTokenForEmployer(Employer)</c>
    /// and <c>CreateTokenForSystemStaff(SystemStaff)</c> — three near-identical bodies that differed
    /// only in which name fields they read. With a common <see cref="User"/> base and roles held in
    /// a join table, one overload covers every actor.
    ///
    /// Refresh-token creation and rotation live in the auth service (Phase 4), not here: this type
    /// is a pure, stateless token factory and is registered as a singleton.
    /// </remarks>
    public interface ITokenService
    {
        AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> roles);

        /// <summary>
        /// Generates a cryptographically random refresh token and the SHA-256 hash to store.
        /// </summary>
        /// <returns>The raw token to hand to the client, and the hash to persist — never the reverse.</returns>
        (string Token, string TokenHash) CreateRefreshToken();

        /// <summary>Hashes a client-presented refresh token so it can be looked up by hash.</summary>
        string HashRefreshToken(string token);
    }
}
