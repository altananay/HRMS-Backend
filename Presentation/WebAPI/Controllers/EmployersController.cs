using Application.Features.Employers.Commands;
using Application.Features.Employers.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class EmployersController : ApiControllerBase
    {
        /// <remarks>
        /// Returns <c>EmployerDto</c>. This endpoint used to serialize the entity directly to
        /// anonymous callers, so <c>PasswordHash</c> and <c>PasswordSalt</c> went out with every
        /// record.
        /// </remarks>
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllEmployerQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [AllowAnonymous]
        [HttpGet("getbyemployerid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdEmployerQuery { Id = id })).Result);

        /// <remarks>Was <c>POST getbyemail</c> — a read modelled as a POST.</remarks>
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("getbyemail")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
            => Ok((await Mediator.Send(new GetByEmailEmployerQuery { Email = email })).Result);

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateEmployerCommand command)
        {
            // An employer may only edit itself; an admin may edit anyone.
            if (!User.IsInRole(Roles.Admin))
            {
                command.Id = CurrentUserId;
            }

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteEmployerCommand { Id = id })).Result);
    }
}
