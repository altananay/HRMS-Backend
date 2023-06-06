using Application.Abstractions;
using Application.Features.JobApplications.Commands;
using Application.Features.JobApplications.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.JobApplications.Commands.CreateJobApplicationCommand;
using static Application.Features.JobApplications.Commands.DeleteJobApplicationCommand;
using static Application.Features.JobApplications.Commands.UpdateJobApplicationCommand;
using static Application.Features.JobApplications.Queries.GetAllByEmployerIdJobApplicationQuery;
using static Application.Features.JobApplications.Queries.GetAllByJobSeekerIdJobApplicationQuery;
using static Application.Features.JobApplications.Queries.GetAllJobApplicationQuery;
using static Application.Features.JobApplications.Queries.GetByIdJobApplicationQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        IMediator _mediator;

        public JobApplicationsController(IMediator mediator, IJobApplicationService jobApplicationService)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllJobApplicationQueryResponse response = await _mediator.Send(new GetAllJobApplicationQuery { });
            if (response.JobApplications.IsSuccess)
            {
                return Ok(response.JobApplications);
            }
            return BadRequest(response.JobApplications);
        }

        [HttpGet("getallbyemployerid/{id}")]
        public async Task<IActionResult> GetAllByEmployerId(string id)
        {
            GetAllByEmployerIdJobApplicationQueryResponse response = await _mediator.Send(new GetAllByEmployerIdJobApplicationQuery { Id = id });
            if (response.JobApplications.IsSuccess)
            {
                return Ok(response.JobApplications);
            }
            return BadRequest(response.JobApplications);
        }

        [HttpGet("getallbyjobseekerid/{id}")]
        public async Task<IActionResult> GetAllByJobSeekerId(string id)
        {
            GetAllByJobSeekerIdJobApplicationQueryResponse response = await _mediator.Send(new GetAllByJobSeekerIdJobApplicationQuery { Id= id });
            if (response.JobApplications.IsSuccess)
            {
                return Ok(response.JobApplications);
            }
            return BadRequest(response.JobApplications);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            GetByIdJobApplicationQueryResponse response = await _mediator.Send(new GetByIdJobApplicationQuery { Id = id });
            if (response.JobApplication.IsSuccess)
            {
                return Ok(response.JobApplication);
            }
            return BadRequest(response.JobApplication);
        }

        [HttpGet("getresultbyid/{id}")]
        public async Task<IActionResult> GetResultById(string id)
        {
            var result = await _mediator.Send(new GetResultByIdJobApplicationQuery { Id= id });
            if (result.Result.IsSuccess)
            {
                return Ok(result.Result);
            }
            return BadRequest(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateJobApplicationCommand jobApplication)
        {
            CreateJobApplicationCommandResponse response = await _mediator.Send(jobApplication);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteJobApplicationCommandResponse response = await _mediator.Send(new DeleteJobApplicationCommand { Id = id });
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobApplicationCommand jobApplication)
        {
            
            UpdateJobApplicationCommandResponse response = await _mediator.Send(jobApplication);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }
    }
}