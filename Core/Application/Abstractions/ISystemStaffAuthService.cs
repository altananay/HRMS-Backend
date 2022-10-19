using Application.Dtos;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface ISystemStaffAuthService
    {
        IDataResult<SystemStaff> Register(SystemStaffForRegisterDto systemStaffForRegisterDto, string password);
        IDataResult<SystemStaff> Login(UserForLoginDto userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(SystemStaff user);
    }
}