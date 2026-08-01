using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WebAPI.Controllers
{
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

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
            => Ok((await Mediator.Send(command)).Result);

        // Every protected page load calls this to verify the session — far higher frequency than any
        // other action here, and unlike them it needs a valid Bearer token just to be reached, so it
        // carries no credential-guessing surface for the "auth" policy to guard. Sharing that bucket
        // meant a few minutes of normal navigation could exhaust it and read as a logout.
        [DisableRateLimiting]
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
            => Ok((await Mediator.Send(new GetCurrentUserQuery { UserId = CurrentUserId })).Result);
    }
}
