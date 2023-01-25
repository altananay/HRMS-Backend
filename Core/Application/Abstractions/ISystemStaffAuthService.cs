using Application.Features.SystemStaffAuth.Queries;
using Application.Features.SystemStaffs.Commands;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface ISystemStaffAuthService
    {
        Task<IResult> Register(CreateSystemStaffCommand createSystemStaffCommand, string password);
        IDataResult<SystemStaff> Login(SystemStaffLoginQuery userForLoginDto);
        IResult UserExists(string email);
        IDataResult<AccessToken> CreateAccessToken(SystemStaff user);
    }
}