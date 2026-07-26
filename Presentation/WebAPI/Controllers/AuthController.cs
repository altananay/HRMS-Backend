using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WebAPI.Controllers
{
    // Never put [AllowAnonymous] on the class: it overrides [Authorize] on individual actions and
    // would silently open /me, /logout-all, /change-password and /register/system-staff.
    [EnableRateLimiting("auth")]
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("register/jobseeker")]
        public async Task<IActionResult> RegisterJobSeeker(RegisterJobSeekerCommand command)
            => Created((string?)null, (await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("register/employer")]
        public async Task<IActionResult> RegisterEmployer(RegisterEmployerCommand command)
            => Created((string?)null, (await Mediator.Send(command)).Result);

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("register/system-staff")]
        public async Task<IActionResult> RegisterSystemStaff(RegisterSystemStaffCommand command)
            => Created((string?)null, (await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
            => Ok((await Mediator.Send(new LogoutAllCommand { UserId = CurrentUserId })).Result);

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            command.UserId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
            => Ok((await Mediator.Send(new GetCurrentUserQuery { UserId = CurrentUserId })).Result);
    }
}
