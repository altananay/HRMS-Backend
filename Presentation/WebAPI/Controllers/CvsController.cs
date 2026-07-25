using Application.Features.Cvs.Commands;
using Application.Features.Cvs.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    public class CvsController : ApiControllerBase
    {
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCvQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [HttpGet("getbyjobseekerid/{jobSeekerId:guid}")]
        public async Task<IActionResult> GetByJobSeekerId(Guid jobSeekerId)
            => Ok((await Mediator.Send(new GetByJobSeekerIdCvQuery { JobSeekerId = jobSeekerId })).Result);

        [Authorize(Roles = Roles.JobSeeker)]
        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateCvCommand command)
        {
            command.JobSeekerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        /// <remarks>
        /// This endpoint returned HTTP 500 unconditionally before: <c>CvManager.Update</c> called a
        /// guard that throws when a CV exists, on an operation that by definition requires one.
        /// </remarks>
        [Authorize(Roles = Roles.JobSeeker)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateCvCommand command)
        {
            command.JobSeekerId = CurrentUserId;

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.JobSeeker)]
        [HttpDelete("deletecv/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteCvCommand { Id = id })).Result);

        // NOTE: the file-upload endpoint is intentionally absent for now.
        //
        // UploadCvFileCommand bypassed the manager layer entirely, wrote CvFile rows with no link to
        // any CV or seeker (the entity had no foreign key at all), returned an empty response type,
        // and its result was discarded by the controller, which always returned Ok(). CvFile now has
        // a required CvId, so the endpoint is reinstated in Phase 5 together with the storage
        // rewrite and content-type/size validation.
    }
}
