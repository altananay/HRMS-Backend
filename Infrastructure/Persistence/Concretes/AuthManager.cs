using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Dtos;
using Application.Results;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using Application.Validators;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class AuthManager : IAuthService
    {
        private IJobSeekerService _jobSeekerService;
        private ITokenHelper _tokenHelper;
        private IUserService _userService;
        public AuthManager(IJobSeekerService jobSeekerService, ITokenHelper tokenHelper, IUserService userService)
        {
            _jobSeekerService = jobSeekerService;
            _tokenHelper = tokenHelper;
            _userService = userService;
        }

        [ValidationAspect(typeof(RegisterValidator))]
        public IResult Register(JobSeekerForRegisterDto userForRegisterDto, string password)
        {
            var user = new User();
            _userService.Add(user);
            string[] claims = { "jobseeker" };
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var jobSeeker = new JobSeeker
            {
                Id = user.Id,
                Email = userForRegisterDto.Email,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                Claims = claims,
                CreatedAt = DateTime.UtcNow,
                DateOfBirth = userForRegisterDto.DateOfBirth,
                NationalityId = userForRegisterDto.IdentityNumber
            };
            var result = _jobSeekerService.Add(jobSeeker);
            if (result.IsSuccess)
            {
                return new SuccessResult(Messages.UserRegistered);
            }
            return new ErrorResult(Messages.CitizenError);
        }

        public IDataResult<JobSeeker> Login(UserForLoginDto userForLoginDto)
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