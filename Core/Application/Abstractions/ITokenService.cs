using Domain.Entities;

namespace Application.Abstractions
{
    public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> roles);

        (string Token, string TokenHash) CreateRefreshToken();

        string HashRefreshToken(string token);
    }
}
