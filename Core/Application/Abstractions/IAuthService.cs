using Application.Dtos;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IAuthService
    {
        IResult Register(JobSeekerForRegisterDto userForRegisterDto, string password);
        IDataResult<JobSeeker> Login(UserForLoginDto userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(JobSeeker user);
    }
}