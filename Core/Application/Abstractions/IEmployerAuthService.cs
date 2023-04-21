using Application.Features.EmployerAuth.Commands;
using Application.Features.EmployerAuth.Queries;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IEmployerAuthService
    {
        Task<IResult> Register(EmployerRegisterCommand userForRegisterDto, string password);
        IDataResult<Employer> Login(EmployerLoginQuery authRequest);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(Employer user);
    }
}