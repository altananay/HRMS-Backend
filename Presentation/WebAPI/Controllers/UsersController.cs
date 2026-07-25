using Application.Features.Users.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : ApiControllerBase
    {
        /// <remarks>
        /// Admin-only and projected to <c>UserSummaryDto</c>. It was anonymous before, and under the
        /// old model returned bare ObjectIds — <c>User</c> was an empty marker entity whose only
        /// purpose was to mint an id for the three actor collections.
        /// </remarks>
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUserQuery query)
            => Ok((await Mediator.Send(query)).Result);
    }
}
