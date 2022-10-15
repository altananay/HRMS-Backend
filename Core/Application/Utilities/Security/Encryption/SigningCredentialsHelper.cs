using Microsoft.IdentityModel.Tokens;

namespace Application.Utilities.Security.Encryption
{
    public class SigningCredentialsHelper
    {
        //Asp.net'e gelen jwt'nin hangi key ile doğrulanacağını ve hangi algoritma ile doğrulacağını belirttik.
        public static SigningCredentials CreateSigningCredentials(SecurityKey securityKey)
        {
            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
        }
    }
}