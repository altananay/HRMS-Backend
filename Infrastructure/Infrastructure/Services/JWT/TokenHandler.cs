using Application.Extensions;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Encryption;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Services.JWT
{
    public class TokenHandler : ITokenHelper
    {
        //IConfiguration interface'i, appsetting.json dosyasındaki konfigürasyonları okumaya yarar.
        public IConfiguration Configuration { get; }
        //TokenOptions sınıfının içerisinde property olarak belirtilen token ayarları
        private TokenOptions _tokenOptions;
        //Token'ın süresi ne zaman bitecek
        private DateTime _accessTokenExpiration;
        public TokenHandler(IConfiguration configuration)
        {
            //dışardan gelen configuration'ı sınıf içerisindeki property olan configuration' ata.
            Configuration = configuration;
            /*appsettings.json dosyasındaki JWT ayarlarının olduğu section olan TokenOptinos section'ını al
            ve TokenOptions sınıfındaki propertyler ile maple*/
            _tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();

        }

        //User bilgisini ve operasyonlar için yetkileri ver.
        public AccessToken CreateToken(JobSeeker user/*, OperationClaim operationClaims*/)
        {
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            var jwt = CreateJwtSecurityToken(_tokenOptions, user, signingCredentials/*, operationClaims*/);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHandler.WriteToken(jwt);

            return new AccessToken
            {
                Token = token,
                Expiration = _accessTokenExpiration
            };

        }

        public JwtSecurityToken CreateJwtSecurityToken(TokenOptions tokenOptions, JobSeeker user,
            SigningCredentials signingCredentials/*, OperationClaim operationClaims*/)
        {
            var jwt = new JwtSecurityToken(
                issuer: tokenOptions.Issuer,
                audience: tokenOptions.Audience,
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: SetClaims(user),
                signingCredentials: signingCredentials
            );
            return jwt;
        }

        private IEnumerable<Claim> SetClaims(JobSeeker user/*, OperationClaim operationClaims*/)
        {
            var claims = new List<Claim>();
            claims.AddNameIdentifier(user.Id);
            claims.AddEmail(user.Email);
            claims.AddName($"{user.FirstName} {user.LastName}");
            claims.AddRoles(user.Claims);

            return claims;
        }
    }
}