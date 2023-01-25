using Application.Abstractions;
using Application.Features.JobApplications.Commands;
using Application.Features.JobApplications.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.JobApplications.Commands.CreateJobApplicationCommand;
using static Application.Features.JobApplications.Commands.DeleteJobApplicationCommand;
using static Application.Features.JobApplications.Commands.UpdateJobApplicationCommand;
using static Application.Features.JobApplications.Queries.GetAllJobApplicationQuery;
using static Application.Features.JobApplications.Queries.GetByIdJobApplicationQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        IMediator _mediator;

        public JobApplicationsController(IMediator mediator)
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