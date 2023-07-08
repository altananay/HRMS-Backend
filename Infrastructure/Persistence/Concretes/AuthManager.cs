using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.JobSeekers.Auth;
using Application.Features.Auth.Queries;
using Application.Features.JobSeekers.Commands;
using Application.Results;
using Application.Utilities.Constants;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using AutoMapper;
using Domain.Entities;
using Persistence.Rules;

namespace Persistence.Concretes
{
    public class AuthManager : IAuthService
    {
        private IJobSeekerService _jobSeekerService;
        private ITokenHelper _tokenHelper;
        private IUserService _userService;
        private IMapper _mapper;
        private JobSeekerAuthBusinessRules _authBusinessRules;
        public AuthManager(IJobSeekerService jobSeekerService, ITokenHelper tokenHelper, IUserService userService, IMapper mapper, JobSeekerAuthBusinessRules jobSeekerAuthBusinessRules)
        {
            _jobSeekerService = jobSeekerService;
            _tokenHelper = tokenHelper;
            _userService = userService;
            _authBusinessRules = jobSeekerAuthBusinessRules;
            _mapper = mapper;
        }

        [ValidationAspect(typeof(RegisterValidator))]
        public async Task<IResult> Register(CreateJobSeekerCommand jobSeekerCommand, string password)
        {
            _authBusinessRules.UserExists(jobSeekerCommand.Email);
            string[] claims = { "jobseeker" };
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            JobSeeker jobSeeker = _mapper.Map<JobSeeker>(jobSeekerCommand);
            jobSeeker.Claims = claims;
            jobSeeker.PasswordHash = passwordHash;
            jobSeeker.PasswordSalt = passwordSalt;
            jobSeeker.Status = true;
            jobSeeker.CreatedAt = DateTime.UtcNow;
            var result = await _jobSeekerService.Add(jobSeeker);
            if (result.IsSuccess)
            {
                return new SuccessResult(Messages.User.UserRegistered);
            }
            return new ErrorResult(Messages.Mernis.CitizenError);
        }

        [ValidationAspect(typeof(JobSeekerLoginAuthValidator))]
        [LogAspect()]
        public IDataResult<JobSeeker> Login(JobSeekerLoginQuery userForLoginDto)
        {
            var userToCheck = _jobSeekerService.GetByMail(userForLoginDto.Email);
            if (userToCheck.Data == null)
            {
                return new ErrorDataResult<JobSeeker>(Messages.User.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<JobSeeker>(Messages.Authentication.PasswordError);
            }

            return new SuccessDataResult<JobSeeker>(userToCheck.Data, Messages.Authentication.SuccessfulLogin);
        }

        public IDataResult<AccessToken> CreateAccessToken(JobSeeker user)
        {
            var accessToken = _tokenHelper.CreateToken(user);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.Authentication.SuccessfulLogin);
        }
    }
}