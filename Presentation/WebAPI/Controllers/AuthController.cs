using Application.Features.Auth.Queries;
using Application.Features.JobSeekers.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Auth.Queries.JobSeekerLoginQuery;
using static Application.Features.JobSeekers.Commands.CreateJobSeekerCommand;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(JobSeekerLoginQuery authQuery)
        {
            JobSeekerLoginQueryResponse response = await _mediator.Send(authQuery);
            if (response.Result == null)
            {
                return Ok(response.DataResult);
            }
            return BadRequest(response.Result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateJobSeekerCommand userForRegisterDto)
        {
            CreateJobSeekerCommandResponse response = await _mediator.Send(userForRegisterDto);
            return Ok(response.Result);
        }
    }
}