using Application.Features.SystemStaffAuth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static Application.Features.SystemStaffAuth.Queries.SystemStaffLoginQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public class SystemStaffAuthController : ControllerBase
    {
        private IMediator _mediator;

        public SystemStaffAuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(SystemStaffLoginQuery request)
        {
            SystemStaffLoginQueryResponse response = await _mediator.Send(request);
            if (!response.SystemStaff.IsSuccess)
            {
                return BadRequest(response.SystemStaff);
            }
            return Ok(response.Token);
        }
    }
}