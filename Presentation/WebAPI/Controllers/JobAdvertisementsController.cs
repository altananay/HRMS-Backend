using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobAdvertisements.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class JobAdvertisementsController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllJobAdvertisementQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [AllowAnonymous]
        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdJobAdvertisementQuery { Id = id })).Result);

        [Authorize(Roles = Roles.Employer)]
        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateJobAdvertisementCommand command)
        {
            command.EmployerId = CurrentUserId;

            var result = (await Mediator.Send(command)).Result;

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [Authorize(Roles = Roles.EmployerOrAdmin)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobAdvertisementCommand command)
        {
            command.EmployerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.EmployerOrAdmin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobAdvertisementCommand
            {
                Id = id,
                EmployerId = CurrentUserId
            })).Result);
    }
}
