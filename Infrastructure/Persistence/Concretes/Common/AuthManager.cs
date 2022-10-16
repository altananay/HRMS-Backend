using Application.Abstractions.Common;
using Application.Aspects;
using Application.Constants;
using Application.Dtos;
using Application.Results;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes.Common
{
    public class AuthManager : IAuthService
    {
        private IJobSeekerService _jobSeekerService;
        private ITokenHelper _tokenHelper;

        public AuthManager(IJobSeekerService userService, ITokenHelper tokenHelper)
        {
            _jobSeekerService = userService;
            _tokenHelper = tokenHelper;
        }

        [ValidationAspect(typeof(RegisterValidator))]
        public IDataResult<JobSeeker> Register(JobSeekerForRegisterDto userForRegisterDto, string password)
        {
            string[] claims = { "jobseeker" };
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new JobSeeker
            {
                Email = userForRegisterDto.Email,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                Claims = claims,
                CreatedAt = DateTime.UtcNow,
            };
            _jobSeekerService.Add(user);
            return new SuccessDataResult<JobSeeker>(user, Messages.UserRegistered);
        }

        public IDataResult<JobSeeker> Login(JobSeekerForLoginDto userForLoginDto)
        {
            var userToCheck = _jobSeekerService.GetByMail(userForLoginDto.Email);
            if (userToCheck == null)
            {
                return new ErrorDataResult<JobSeeker>(Messages.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<JobSeeker>(Messages.PasswordError);
            }

            return new SuccessDataResult<JobSeeker>(userToCheck.Data, Messages.SuccessfulLogin);
        }

        public IResult UserExists(string email)
        {
            if (_jobSeekerService.GetByMail(email).Data != null)
            {
                return new ErrorResult(Messages.UserAlreadyExists);
            }
            return new SuccessResult();
        }

        public IDataResult<AccessToken> CreateAccessToken(JobSeeker user)
        {
            var accessToken = _tokenHelper.CreateToken(user);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.SuccessfulLogin);
        }
    }
}