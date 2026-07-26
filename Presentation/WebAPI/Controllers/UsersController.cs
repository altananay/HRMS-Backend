using Application.Features.Users.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : ApiControllerBase
    {
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUserQuery query)
            => Ok((await Mediator.Send(query)).Result);
    }
}
