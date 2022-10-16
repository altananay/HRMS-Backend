using Application.Dtos;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions.Common
{
    public interface IAuthService
    {
        IDataResult<JobSeeker> Register(JobSeekerForRegisterDto userForRegisterDto, string password);
        IDataResult<JobSeeker> Login(JobSeekerForLoginDto userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(JobSeeker user);
    }
}