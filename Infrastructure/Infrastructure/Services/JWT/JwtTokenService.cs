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
    /// <summary>
    /// Stateless JWT factory. Registered as a singleton.
    /// </summary>
    /// <remarks>
    /// The previous TokenHandler was registered <c>AddScoped</c> in the MS container but injected
    /// into managers that Autofac registered <c>.SingleInstance()</c> — a captive dependency — and
    /// it kept the expiry in a mutable <c>_accessTokenExpiration</c> instance field that concurrent
    /// requests raced on. This type holds no per-request state, so it is safe as a singleton.
    /// </remarks>
    public sealed class JwtTokenService : ITokenService
    {
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
            // UtcNow via TimeProvider. The old code used DateTime.Now for both nbf and exp, so on a
            // UTC+3 server every token was stamped three hours ahead and rejected by
            // ValidateLifetime as not-yet-valid — well beyond the default five-minute skew.
            var issuedAt = _timeProvider.GetUtcNow().UtcDateTime;
            var expiresAt = issuedAt.AddMinutes(_options.AccessTokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new(ClaimTypes.Email, user.Email),
                new("user_type", user.UserType.ToString()),
                new("security_stamp", user.SecurityStamp.ToString())
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

        public (string Token, string TokenHash) CreateRefreshToken()
        {
            // 256 bits from a cryptographic RNG. Base64url so it survives headers and JSON intact.
            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Base64UrlEncoder.Encode(bytes);

            return (token, HashRefreshToken(token));
        }

        /// <remarks>
        /// Only the hash is ever persisted. A leaked <c>refresh_tokens</c> table must not hand an
        /// attacker usable sessions, which storing raw tokens would. SHA-256 with no salt is correct
        /// here and not a password-hashing mistake: the input is 256 bits of entropy we generated,
        /// so it is not brute-forcible and the lookup must be deterministic.
        /// </remarks>
        public string HashRefreshToken(string token)
            => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
