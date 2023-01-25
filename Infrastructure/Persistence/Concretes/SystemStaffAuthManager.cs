using Application.Abstractions;
using Application.Aspects;
using Application.Constants;
using Application.Features.SystemStaffAuth.Queries;
using Application.Features.SystemStaffs.Commands;
using Application.Results;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using Application.Validators.SystemStaffs.Auth;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class SystemStaffAuthManager : ISystemStaffAuthService
    {
        private ISystemStaffService _systemStaffService;
        private ITokenHelper _tokenHelper;

        public SystemStaffAuthManager(ISystemStaffService systemStaffService, ITokenHelper tokenHelper)
        {
            _systemStaffService = systemStaffService;
            _tokenHelper = tokenHelper;
        }

        public IDataResult<AccessToken> CreateAccessToken(SystemStaff user)
        {
            var accessToken = _tokenHelper.CreateTokenForSystemStaff(user);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.SuccessfulLogin);
        }

        [ValidationAspect(typeof(SystemStaffLoginAuthValidator))]
        public IDataResult<SystemStaff> Login(SystemStaffLoginQuery userForLoginDto)
        {
            var userToCheck = _systemStaffService.GetByEmail(userForLoginDto.Email);
            if (userToCheck == null)
            {
                return new ErrorDataResult<SystemStaff>(Messages.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<SystemStaff>(Messages.PasswordError);
            }

            return new SuccessDataResult<SystemStaff>(userToCheck.Data, Messages.SuccessfulLogin);
        }

        [ValidationAspect(typeof(CreateSystemStaffCommand))]
        public async Task<IResult> Register(CreateSystemStaffCommand createSystemStaffCommand, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new SystemStaff
            {
                Email = createSystemStaffCommand.Email,
                FirstName = createSystemStaffCommand.FirstName,
                LastName = createSystemStaffCommand.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                Claims = createSystemStaffCommand.Claims,
                CreatedAt = DateTime.UtcNow,
            };
            await _systemStaffService.Add(user);
            return new SuccessResult(Messages.SystemStaffAdded);
        }

        public IResult UserExists(string email)
        {
            if (_systemStaffService.GetByEmail(email).Data != null)
            {
                return new ErrorResult(Messages.UserAlreadyExists);
            }
            return new SuccessResult();
        }
    }
}