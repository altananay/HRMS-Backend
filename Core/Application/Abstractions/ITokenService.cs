using Domain.Entities;

namespace Application.Abstractions
{
    public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> roles);

        /// <summary>
        /// 256 bits of cryptographic randomness plus its hash. Backs both refresh tokens and
        /// password-reset tokens — same primitive, and neither is ever stored in the clear.
        /// </summary>
        (string Token, string TokenHash) CreateSecureToken();

        string HashToken(string token);
    }
}
