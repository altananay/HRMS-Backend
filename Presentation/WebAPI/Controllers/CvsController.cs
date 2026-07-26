using Application.Abstractions.Services;
using Application.Abstractions.Storage;
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

        /// <summary>Reads a candidate's CV, for a caller entitled to see it.</summary>
        /// <remarks>
        /// A CV is personal data, so this cannot be a plain lookup by id. The rule lives in
        /// <c>CandidateAccessPolicy</c> — owner, an employer holding an application from them, or an
        /// admin — the same one the file download has always used.
        /// </remarks>
        [HttpGet("getbyjobseekerid/{jobSeekerId:guid}")]
        public async Task<IActionResult> GetByJobSeekerId(Guid jobSeekerId)
            => Ok((await Mediator.Send(new GetByJobSeekerIdCvQuery
            {
                JobSeekerId = jobSeekerId,
                RequestedBy = CurrentUserId
            })).Result);

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
            => Ok((await Mediator.Send(new DeleteCvCommand { Id = id, RequestedBy = CurrentUserId })).Result);

        /// <summary>Attaches document files to the caller's own CV.</summary>
        [Authorize(Roles = Roles.JobSeeker)]
        [HttpPost("uploadfile")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile(IFormFileCollection files)
        {
            // IFormFile is translated here so the Application layer never sees an ASP.NET type —
            // the storage abstraction used to take IFormFileCollection directly, which is why
            // Application had to reference the whole ASP.NET Core shared framework.
            var command = new UploadCvFileCommand
            {
                JobSeekerId = CurrentUserId,
                Files = files.Select(file => new FileUploadRequest(
                    file.FileName, file.ContentType, file.Length, file.OpenReadStream())).ToList()
            };

            return Ok((await Mediator.Send(command)).Result);
        }

        /// <summary>
        /// Streams a CV attachment to a caller entitled to read it.
        /// </summary>
        /// <remarks>
        /// A proxy rather than a redirect to storage. CVs are personal data, so no URL that works
        /// without the caller's token is ever produced — which also rules out presigned/SAS links.
        /// Permission is decided in <c>CvFileManager</c>, since "an employer who received an
        /// application from this seeker" is a data question, not a role check.
        /// </remarks>
        [Authorize]
        [HttpGet("files/{id:guid}")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var response = await Mediator.Send(new DownloadCvFileQuery { Id = id, RequestedBy = CurrentUserId });

            return File(response.Download.Content, response.Download.ContentType, response.Download.FileName);
        }

        [Authorize]
        [HttpDelete("files/{id:guid}")]
        public async Task<IActionResult> DeleteFile(Guid id)
            => Ok((await Mediator.Send(new DeleteCvFileCommand { Id = id, RequestedBy = CurrentUserId })).Result);
    }
}
