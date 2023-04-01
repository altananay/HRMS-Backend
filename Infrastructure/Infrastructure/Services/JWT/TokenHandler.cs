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
        public AccessToken CreateToken(JobSeeker jobSeeker)
        {
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            var jwt = CreateJwtSecurityToken(_tokenOptions, jobSeeker, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHandler.WriteToken(jwt);


            return new AccessToken
            {
                Token = token,
                Expiration = _accessTokenExpiration
            };

        }

        public AccessToken CreateTokenForSystemStaff(SystemStaff systemStaff)
        {
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            var jwt = CreateJwtSecurityTokenForSystemStaff(_tokenOptions, systemStaff, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHandler.WriteToken(jwt);

            return new AccessToken
            {
                Token = token,
                Expiration = _accessTokenExpiration
            };

        }

        public JwtSecurityToken CreateJwtSecurityToken(TokenOptions tokenOptions, JobSeeker jobSeeker,
            SigningCredentials signingCredentials)
        {
            var jwt = new JwtSecurityToken(
                issuer: tokenOptions.Issuer,
                audience: tokenOptions.Audience,
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: SetClaims(jobSeeker),
                signingCredentials: signingCredentials
            );
            return jwt;
        }

        public JwtSecurityToken CreateJwtSecurityTokenForSystemStaff(TokenOptions tokenOptions, SystemStaff systemStaff,
            SigningCredentials signingCredentials)
        {
            var jwt = new JwtSecurityToken(
                issuer: tokenOptions.Issuer,
                audience: tokenOptions.Audience,
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: SetClaimsForSystemStaff(systemStaff),
                signingCredentials: signingCredentials
            );
            return jwt;
        }

        public JwtSecurityToken CreateJwtSecurityTokenForEmployer(TokenOptions tokenOptions, Employer employer,
            SigningCredentials signingCredentials)
        {
            var jwt = new JwtSecurityToken(
                issuer: tokenOptions.Issuer,
                audience: tokenOptions.Audience,
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: SetClaimsForEmployer(employer),
                signingCredentials: signingCredentials
            );
            return jwt;
        }

        private IEnumerable<Claim> SetClaims(JobSeeker jobSeeker)
        {
            var claims = new List<Claim>();
            claims.AddNameIdentifier(jobSeeker.Id);
            claims.AddEmail(jobSeeker.Email);
            claims.AddName($"{jobSeeker.FirstName} {jobSeeker.LastName}");
            claims.AddRoles(jobSeeker.Claims);

            return claims;
        }

        private IEnumerable<Claim> SetClaimsForEmployer(Employer employer)
        {
            var claims = new List<Claim>();
            claims.AddNameIdentifier(employer.Id);
            claims.AddEmail(employer.Email);
            claims.AddName($"{employer.CompanyName}");
            claims.AddRoles(employer.Claims);

            return claims;
        }

        private IEnumerable<Claim> SetClaimsForSystemStaff(SystemStaff systemStaff)
        {
            var claims = new List<Claim>();
            claims.AddNameIdentifier(systemStaff.Id);
            claims.AddEmail(systemStaff.Email);
            claims.AddName($"{systemStaff.FirstName} {systemStaff.LastName}");
            claims.AddRoles(systemStaff.Claims);

            return claims;
        }

        public AccessToken CreateTokenForEmployer(Employer employer)
        {
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            var jwt = CreateJwtSecurityTokenForEmployer(_tokenOptions, employer, signingCredentials);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtSecurityTokenHandler.WriteToken(jwt);

            return new AccessToken
            {
                Token = token,
                Expiration = _accessTokenExpiration
            };
        }
    }
}