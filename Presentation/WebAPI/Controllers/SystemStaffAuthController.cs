using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemStaffAuthController : ControllerBase
    {
        private ISystemStaffAuthService _systemStaffAuthService;

        public SystemStaffAuthController(ISystemStaffAuthService systemStaffAuthService)
        {
            _systemStaffAuthService = systemStaffAuthService;
        }

        [HttpPost("login")]
        public ActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _systemStaffAuthService.Login(userForLoginDto);
            if (!userToLogin.IsSuccess)
            {
                return BadRequest(userToLogin.Message);
            }

            var result = _systemStaffAuthService.CreateAccessToken(userToLogin.Data);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result.Message);
        }

        [HttpPost("register")]
        public ActionResult Register(SystemStaffForRegisterDto userForRegisterDto)
        {
            var userExists = _systemStaffAuthService.UserExists(userForRegisterDto.Email);
            if (!userExists.IsSuccess)
            {
                return BadRequest(userExists.Message);
            }

            var registerResult = _systemStaffAuthService.Register(userForRegisterDto, userForRegisterDto.Password);
            if (registerResult.IsSuccess)
            {
                return Ok(registerResult);
            }

            return BadRequest(registerResult.Message);
        }
    }
}
