using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Utilities.Helpers
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(JobSeeker user/*, OperationClaim operationClaims*/);
    }
}