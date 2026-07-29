using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Application.Utilities.JWT;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.JWT
{
    public sealed class JwtTokenService : ITokenService
    {
        public const string TokenSchemaVersionClaim = "ver";

        public const string CurrentTokenSchemaVersion = "1";

        private readonly TokenOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(IOptions<TokenOptions> options, TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey));
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public AccessToken CreateAccessToken(User user, IReadOnlyCollection<string> roles)
        {
            var issuedAt = _timeProvider.GetUtcNow().UtcDateTime;
            var expiresAt = issuedAt.AddMinutes(_options.AccessTokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new(ClaimTypes.Email, user.Email),
                new("user_type", user.UserType.ToString()),
                new("security_stamp", user.SecurityStamp.ToString()),

                new(TokenSchemaVersionClaim, CurrentTokenSchemaVersion)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: _signingCredentials);

            return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        public (string Token, string TokenHash) CreateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Base64UrlEncoder.Encode(bytes);

            return (token, HashToken(token));
        }

        public string HashToken(string token)
            => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
