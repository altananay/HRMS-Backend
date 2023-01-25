using Application.Abstractions;
using Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Users.Queries.GetAllUserQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllUserQueryResponse response = await _mediator.Send(new GetAllUserQuery { });
            if (response.Users.IsSuccess)
            {
                return Ok(response.Users);
            }
            return BadRequest(response.Users);
        }
    }
}
