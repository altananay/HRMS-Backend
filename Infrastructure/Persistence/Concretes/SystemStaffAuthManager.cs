using Application.Abstractions;
using Application.Constants;
using Application.Dtos;
using Application.Results;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
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

        public IDataResult<SystemStaff> Login(UserForLoginDto userForLoginDto)
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

        public IDataResult<SystemStaff> Register(SystemStaffForRegisterDto systemStaffForRegisterDto, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new SystemStaff
            {
                Email = systemStaffForRegisterDto.Email,
                FirstName = systemStaffForRegisterDto.FirstName,
                LastName = systemStaffForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                Claims = systemStaffForRegisterDto.Claims,
                CreatedAt = DateTime.UtcNow,
            };
            _systemStaffService.Add(user);
            return new SuccessDataResult<SystemStaff>(user, Messages.SystemStaffAdded);
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