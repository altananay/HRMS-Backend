using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.Employers.Auth;
using Application.Features.EmployerAuth.Commands;
using Application.Features.EmployerAuth.Queries;
using Application.Results;
using Application.Utilities.Constants;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using AutoMapper;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class EmployerAuthManager : IEmployerAuthService
    {
        private IEmployerService _employerService;
        private ITokenHelper _tokenHelper;
        private IMapper _mapper;

        public EmployerAuthManager(IEmployerService employerService, ITokenHelper tokenHelper, IMapper mapper)
        {
            _employerService = employerService;
            _tokenHelper = tokenHelper;
            _mapper = mapper;
        }

        public IDataResult<AccessToken> CreateAccessToken(Employer user)
        {
            var accessToken = _tokenHelper.CreateTokenForEmployer(user);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.Authentication.SuccessfulLogin);
        }

        [ValidationAspect(typeof(EmployerLoginAuthValidator))]
        [LogAspect()]
        public IDataResult<Employer> Login(EmployerLoginQuery loginRequest)
        {
            var userToCheck = _employerService.GetByEmail(loginRequest.Email);
            if (userToCheck.Data == null)
            {
                return new ErrorDataResult<Employer>(Messages.User.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(loginRequest.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<Employer>(Messages.Authentication.PasswordError);
            }

            return new SuccessDataResult<Employer>(userToCheck.Data, Messages.Authentication.SuccessfulLogin);
        }

        [ValidationAspect(typeof(EmployerValidator))]
        public async Task<IResult> Register(EmployerRegisterCommand employerRegister, string password)
        {
            string[] claims = { "employer" };
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            Employer employer = _mapper.Map<Employer>(employerRegister);
            employer.Claims = claims;
            employer.PasswordHash = passwordHash;
            employer.PasswordSalt = passwordSalt;
            employer.Status = true;
            employer.CreatedAt = DateTime.UtcNow;
            await _employerService.Add(employer);
            return new SuccessResult(Messages.User.UserRegistered);
        }

        public IResult UserExists(string email)
        {
            if (_employerService.GetByEmail(email).Data != null)
            {
                return new ErrorResult(Messages.Authentication.UserAlreadyExists);
            }
            return new SuccessResult();
        }
    }
}