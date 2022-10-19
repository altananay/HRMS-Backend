using Application.Utilities.JWT;
using Domain.Entities;

namespace Application.Utilities.Helpers
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(JobSeeker jobSeeker);
        AccessToken CreateTokenForEmployer(Employer employer);
        AccessToken CreateTokenForSystemStaff(SystemStaff systemStaff);
    }
}