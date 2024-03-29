﻿using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Validation.Validators.SystemStaffs.Auth;
using Application.Features.SystemStaffAuth.Queries;
using Application.Features.SystemStaffs.Commands;
using Application.Results;
using Application.Utilities.Constants;
using Application.Utilities.Helpers;
using Application.Utilities.JWT;
using Application.Utilities.Security.Hashing;
using AutoMapper;
using Domain.Entities;

namespace Persistence.Concretes
{
    public class SystemStaffAuthManager : ISystemStaffAuthService
    {
        private ISystemStaffService _systemStaffService;
        private ITokenHelper _tokenHelper;
        private IMapper _mapper;

        public SystemStaffAuthManager(ISystemStaffService systemStaffService, ITokenHelper tokenHelper, IMapper mapper)
        {
            _systemStaffService = systemStaffService;
            _tokenHelper = tokenHelper;
            _mapper = mapper;
        }

        public IDataResult<AccessToken> CreateAccessToken(SystemStaff user)
        {
            var accessToken = _tokenHelper.CreateTokenForSystemStaff(user);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.Authentication.SuccessfulLogin);
        }

        [ValidationAspect(typeof(SystemStaffLoginAuthValidator))]
        public IDataResult<SystemStaff> Login(SystemStaffLoginQuery userForLoginDto)
        {
            var userToCheck = _systemStaffService.GetByEmail(userForLoginDto.Email);
            if (userToCheck == null)
            {
                return new ErrorDataResult<SystemStaff>(Messages.User.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.Data.PasswordHash, userToCheck.Data.PasswordSalt))
            {
                return new ErrorDataResult<SystemStaff>(Messages.Authentication.PasswordError);
            }

            return new SuccessDataResult<SystemStaff>(userToCheck.Data, Messages.Authentication.SuccessfulLogin);
        }

        [ValidationAspect(typeof(CreateSystemStaffCommand))]
        public async Task<IResult> Register(CreateSystemStaffCommand createSystemStaffCommand, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var systemStaff = _mapper.Map<SystemStaff>(createSystemStaffCommand);
            systemStaff.PasswordHash = passwordHash;
            systemStaff.PasswordSalt = passwordSalt;
            systemStaff.Status = true;
            systemStaff.CreatedAt = DateTime.UtcNow;
            await _systemStaffService.Add(systemStaff);
            return new SuccessResult(Messages.SystemStaff.SystemStaffAdded);
        }

        public IResult UserExists(string email)
        {
            if (_systemStaffService.GetByEmail(email).Data != null)
            {
                return new ErrorResult(Messages.Authentication.UserAlreadyExists);
            }
            return new SuccessResult();
        }
    }
}