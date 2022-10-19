using Application.Dtos;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IEmployerAuthService
    {
        IDataResult<Employer> Register(EmployerForRegisterDto userForRegisterDto, string password);
        IDataResult<Employer> Login(UserForLoginDto userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(Employer user);
    }
}