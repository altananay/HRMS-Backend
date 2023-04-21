using Application.Abstractions;
using Application.Aspects;
using Application.Aspects.AutofacAspects;
using Application.Constants;
using Application.Features.EmployerAuth.Commands;
using Application.Features.EmployerAuth.Queries;
using Application.Results;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using Application.Validators.Employers.Auth;
using Domain.Entities;

namespace Persistence.Concretes
{
    internal class EmployerAuthManager : IEmployerAuthService
    {
        private IEmployerService _employerService;
        private ITokenHelper _tokenHelper;

        public EmployerAuthManager(IEmployerService employerService, ITokenHelper tokenHelper)
        {
            _employerService = employerService;
            _tokenHelper = tokenHelper;
        }

        public IDataResult<AccessToken> CreateAccessToken(Employer user)
        {
            var accessToken = _tokenHelper.CreateTokenForEmployer(user);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.SuccessfulLogin);
        }

        [ValidationAspect(typeof(EmployerLoginAuthValidator))]
        public IDataResult<Employer> Login(EmployerLoginQuery loginRequest)
        {
            var userToCheck = _employerService.GetByEmail(loginRequest.Email);
            if (userToCheck.Data == null)
            {
                return new ErrorDataResult<Employer>(Messages.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(loginRequest.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<Employer>(Messages.PasswordError);
            }

            return new SuccessDataResult<Employer>(userToCheck.Data, Messages.SuccessfulLogin);
        }

        [ValidationAspect(typeof(EmployerValidator))]
        public async Task<IResult> Register(EmployerRegisterCommand userForRegisterDto, string password)
        {
            string[] claims = { "employer" };
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new Employer
            {
                CompanyName = userForRegisterDto.CompanyName,
                CompanyPhone = userForRegisterDto.CompanyPhone,
                WebSite = userForRegisterDto.WebSite,
                Email = userForRegisterDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                Claims = claims,
                CreatedAt = DateTime.UtcNow,
            };
            await _employerService.Add(user);
            return new SuccessResult(Messages.UserRegistered);
        }

        public IResult UserExists(string email)
        {
            if (_employerService.GetByEmail(email).Data != null)
            {
                return new ErrorResult(Messages.UserAlreadyExists);
            }
            return new SuccessResult();
        }
    }
}
