using Application.Features.Auth.Queries;
using Application.Features.JobSeekers.Commands;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IAuthService
    {
        Task<IResult> Register(CreateJobSeekerCommand userForRegisterDto, string password);
        IDataResult<JobSeeker> Login(JobSeekerLoginQuery userForLoginDto);
        IDataResult<AccessToken> CreateAccessToken(JobSeeker user);
    }
}