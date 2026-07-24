using Application.Features.JobPositions.Commands;
using Application.Features.JobPositions.Queries;
using MediatR;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.JobPositions.Commands.CreateJobPositionCommand;
using static Application.Features.JobPositions.Commands.DeleteJobPositionCommand;
using static Application.Features.JobPositions.Commands.UpdateJobPositionCommand;
using static Application.Features.JobPositions.Queries.GetJobPositionByIdQuery;
using static Application.Features.JobPositions.Queries.GetJobPositionQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobPositionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobPositionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("addjobposition")]
        public async Task<IActionResult> Add(CreateJobPositionCommand jobPosition)
        {
            CreateJobPositionCommandResponse response = await _mediator.Send(jobPosition);
            return Ok(response.Result);
        }

        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetJobPositionQueryResponse response = await _mediator.Send(new GetJobPositionQuery { });
            return Ok(response.jobPositions);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteJobPositionCommandResponse response = await _mediator.Send(new DeleteJobPositionCommand { Id = id});
            return Ok(response.Result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobPositionCommand jobPosition)
        {
            UpdateJobPositionCommandResponse response = await _mediator.Send(jobPosition);
            return Ok(response.Result);
        }

        [AllowAnonymous]
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            GetJobPositionByIdQueryResponse response = await _mediator.Send(new GetJobPositionByIdQuery { Id = id});
            return Ok(response.JobPosition);
        }
    }
}
