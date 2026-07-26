using Application.Features.JobApplications.Commands;
using Application.Features.JobApplications.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class JobApplicationsController : ApiControllerBase
    {
        /// <summary>
        /// Applications, scoped to whoever is asking.
        /// </summary>
        /// <remarks>
        /// A job seeker sees only their own and an employer only those against their own
        /// advertisements — the filter is derived from the token rather than from a route parameter.
        /// Previously <c>getallbyemployerid/{id}</c> and <c>getallbyjobseekerid/{id}</c> took the id
        /// from the URL with no ownership check and no authentication at all, so anyone could read
        /// anyone's applications by iterating ids.
        /// </remarks>
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllJobApplicationQuery query)
        {
            if (User.IsInRole(Roles.Employer))
            {
                query.EmployerId = CurrentUserId;
                query.JobSeekerId = null;
            }
            else if (User.IsInRole(Roles.JobSeeker))
            {
                query.JobSeekerId = CurrentUserId;
                query.EmployerId = null;
            }

            // Admins see everything, so their filters are left as supplied.
            return Ok((await Mediator.Send(query)).Result);
        }

        /// <remarks>
        /// GetAll above narrows by role and token; this took an id and served it to any
        /// authenticated caller, so iterating ids exposed every applicant's name and every
        /// employer's private note. The service now admits only the two parties, or an admin.
        /// </remarks>
        [HttpGet("getbyid/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdJobApplicationQuery
            {
                Id = id,
                RequestedBy = CurrentUserId
            })).Result);

        [Authorize(Roles = Roles.JobSeeker)]
        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateJobApplicationCommand command)
        {
            command.JobSeekerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        /// <summary>Employer-side moderation: set the status and leave a note.</summary>
        [Authorize(Roles = Roles.Employer)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobApplicationCommand command)
        {
            command.EmployerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteJobApplicationCommand { Id = id })).Result);
    }
}
