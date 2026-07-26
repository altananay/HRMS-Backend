using Application.Features.JobPositions.Commands;
using Application.Features.JobPositions.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class JobPositionController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetJobPositionQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [AllowAnonymous]
        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetJobPositionByIdQuery { Id = id })).Result);

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("addjobposition")]
        public async Task<IActionResult> Add(CreateJobPositionCommand command)
        {
            var result = (await Mediator.Send(command)).Result;

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobPositionCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobPositionCommand { Id = id })).Result);
    }
}
