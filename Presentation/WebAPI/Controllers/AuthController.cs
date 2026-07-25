using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Registration, sign-in and token lifecycle for every actor type.
    /// </summary>
    /// <remarks>
    /// Replaces <c>AuthController</c>, <c>EmployerAuthController</c> and
    /// <c>SystemStaffAuthController</c>. Three login endpoints over one user table is a footgun: a
    /// job seeker posting to the employer endpoint would get a different error than a wrong password,
    /// which is an enumeration signal by construction. One endpoint, one uniform 401, and the
    /// <c>userType</c> in the response tells the client where to route.
    /// </remarks>
    // NOT [AllowAnonymous] at class level, deliberately. ASP.NET Core treats [AllowAnonymous]
    // anywhere in an endpoint's metadata as final, so a class-level one silently overrides
    // [Authorize] on individual actions — which made /me, /logout-all and /change-password
    // anonymous. Each action opts out explicitly instead.
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
            => Ok((await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("register/employer")]
        public async Task<IActionResult> RegisterEmployer(RegisterEmployerCommand command)
            => Ok((await Mediator.Send(command)).Result);

        /// <remarks>
        /// Admin-only, and the role is assigned server-side. The old equivalent took a
        /// client-supplied <c>Claims</c> array that AutoMapper copied onto the entity.
        /// </remarks>
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("register/system-staff")]
        public async Task<IActionResult> RegisterSystemStaff(RegisterSystemStaffCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenCommand command)
            => Ok((await Mediator.Send(command)).Result);

        /// <remarks>
        /// Anonymous by design: the refresh token itself is the credential, and a client whose
        /// access token has already expired must still be able to end its session.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommand command)
            => Ok((await Mediator.Send(command)).Result);

        /// <summary>Ends every session for the caller, including unexpired access tokens.</summary>
        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
            => Ok((await Mediator.Send(new LogoutAllCommand { UserId = CurrentUserId })).Result);

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            // Always the caller. A user id in the body would let anyone reset anyone's password.
            command.UserId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
            => Ok((await Mediator.Send(new GetCurrentUserQuery { UserId = CurrentUserId })).Result);
    }
}
