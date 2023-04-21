using Application.Features.EmployerAuth.Commands;
using Application.Features.EmployerAuth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.EmployerAuth.Commands.EmployerRegisterCommand;
using static Application.Features.EmployerAuth.Queries.EmployerLoginQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployerAuthController : ControllerBase
    {
        private IMediator _mediator;

        public EmployerAuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(EmployerLoginQuery loginRequest)
        {
            EmployerLoginQueryResponse response = await _mediator.Send(loginRequest);
            if (!response.Employer.IsSuccess)
            {
                return BadRequest(response.Employer);
            }
            return Ok(response.Token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(EmployerRegisterCommand employerRegisterRequest)
        {
            EmployerRegisterCommandResponse response = await _mediator.Send(employerRegisterRequest);
            if (response.Employer == null)
            {
                return BadRequest(response.Employer);
            }

            if (response.Employer.IsSuccess)
            {
                return Ok(response.Employer);
            }

            return BadRequest(response.Employer);
        }
    }
}
