using Application.Features.JobSeekers.Commands;
using Application.Features.JobSeekers.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class JobSeekersController : ApiControllerBase
    {
        /// <remarks>
        /// Admin-only, and returns <c>JobSeekerDto</c>. This was the worst leak in the API: fully
        /// anonymous, serializing raw entities, so every job seeker's <c>PasswordHash</c>,
        /// <c>PasswordSalt</c> and national ID were available to anyone who called it.
        /// </remarks>
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllJobSeekerQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdJobSeekerQuery { Id = id })).Result);

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("getbyemail")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
            => Ok((await Mediator.Send(new GetByEmailJobSeekerQuery { Email = email })).Result);

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobSeekerCommand command)
        {
            if (!User.IsInRole(Roles.Admin))
            {
                command.Id = CurrentUserId;
            }

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobSeekerCommand { Id = id })).Result);
    }
}
