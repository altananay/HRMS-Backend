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
        /// <summary>Public: the job board needs the position list to render its filters.</summary>
        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetJobPositionQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [AllowAnonymous]
        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetJobPositionByIdQuery { Id = id })).Result);

        /// <remarks>
        /// Resolve-or-create, so re-adding an existing name is idempotent and answers 201 carrying
        /// the id of the position that was already there.
        /// </remarks>
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

        /// <remarks>Returns 409 when advertisements still reference this position.</remarks>
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobPositionCommand { Id = id })).Result);
    }
}
