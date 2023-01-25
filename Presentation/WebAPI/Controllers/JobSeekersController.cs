using Application.Features.JobSeekers.Commands;
using Application.Features.JobSeekers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.JobSeekers.Commands.CreateJobSeekerCommand;
using static Application.Features.JobSeekers.Commands.DeleteJobSeekerCommand;
using static Application.Features.JobSeekers.Commands.UpdateJobSeekerCommand;
using static Application.Features.JobSeekers.Queries.GetAllJobSeekerQuery;
using static Application.Features.JobSeekers.Queries.GetByEmailJobSeekerQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSeekersController : ControllerBase
    {
        
        IMediator _mediator;

        public JobSeekersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllJobSeekerCommandResponse response = await _mediator.Send(new GetAllJobSeekerQuery { });
            if (response.JobSeekers.IsSuccess)
            {
                return Ok(response.JobSeekers);
            }
            return BadRequest(response.JobSeekers);
        }

        [HttpPost("getbyemail")]
        public async Task<IActionResult> GetByEmail(GetByEmailJobSeekerQuery request)
        {
            GetByEmailJobSeekerResponse response = await _mediator.Send(request);
            if (response.JobSeeker.IsSuccess)
            {
                return Ok(response.JobSeeker);
            }
            return BadRequest(response.JobSeeker);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteJobSeekerCommandResponse response = await _mediator.Send(new DeleteJobSeekerCommand { Id = id});
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobSeekerCommand jobSeeker)
        {
            UpdateJobSeekerCommandResponse result = await _mediator.Send(jobSeeker);
            if (result.Result.IsSuccess)
            {
                return Ok(result.Result);
            }
            return BadRequest(result.Result);
        }
    }
}