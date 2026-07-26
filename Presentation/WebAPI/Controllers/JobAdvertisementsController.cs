using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobAdvertisements.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <remarks>
    /// The six near-identical GET endpoints (getall, getallbystatus, getallorderbysalary,
    /// getbyemployerid, getbyemployerid/{id}/{status}) collapse into one parameterised query. Two of
    /// the originals also passed an employer id to <c>JobAdvertisementExists</c>, which validates
    /// advertisement ids — so they threw for every caller.
    /// </remarks>
    [Authorize]
    public class JobAdvertisementsController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllJobAdvertisementQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [AllowAnonymous]
        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdJobAdvertisementQuery { Id = id })).Result);

        [Authorize(Roles = Roles.Employer)]
        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateJobAdvertisementCommand command)
        {
            // Taken from the token, never the body. It used to be a client-supplied field, so any
            // caller could publish an advertisement in any employer's name.
            command.EmployerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.Employer)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobAdvertisementCommand command)
        {
            // The service compares this against the advertisement's owner and throws Forbidden on a
            // mismatch, so an employer cannot edit someone else's listing.
            command.EmployerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        /// <remarks>
        /// Same ownership guard as Update. It was missing here, so the role attribute was the only
        /// thing standing between a rival employer and someone else's listing — and it grants
        /// exactly the role every attacker in this scenario already holds.
        /// </remarks>
        [Authorize(Roles = Roles.Employer)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobAdvertisementCommand
            {
                Id = id,
                EmployerId = CurrentUserId
            })).Result);
    }
}
