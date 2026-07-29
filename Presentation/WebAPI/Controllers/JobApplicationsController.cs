using Application.Features.JobApplications.Commands;
using Application.Features.JobApplications.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class JobApplicationsController : ApiControllerBase
    {
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllJobApplicationQuery query)
        {
            if (User.IsInRole(Roles.Employer))
            {
                query.EmployerId = CurrentUserId;
                query.JobSeekerId = null;
            }
            else if (User.IsInRole(Roles.JobSeeker))
            {
                query.JobSeekerId = CurrentUserId;
                query.EmployerId = null;
            }

            return Ok((await Mediator.Send(query)).Result);
        }

        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdJobApplicationQuery
            {
                Id = id,
                RequestedBy = CurrentUserId
            })).Result);

        [Authorize(Roles = Roles.JobSeeker)]
        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateJobApplicationCommand command)
        {
            command.JobSeekerId = CurrentUserId;

            var result = (await Mediator.Send(command)).Result;

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [Authorize(Roles = Roles.EmployerOrAdmin)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobApplicationCommand command)
        {
            command.EmployerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobApplicationCommand { Id = id })).Result);
    }
}
