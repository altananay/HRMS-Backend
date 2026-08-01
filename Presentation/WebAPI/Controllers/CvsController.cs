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

            var result = (await Mediator.Send(command)).Result;

            return CreatedAtAction(
                nameof(GetByJobSeekerId), new { jobSeekerId = command.JobSeekerId }, result);
        }

        [Authorize(Roles = Roles.JobSeekerOrAdmin)]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateCvCommand command)
        {
            if (!User.IsInRole(Roles.Admin))
            {
                command.JobSeekerId = CurrentUserId;
            }

            return Ok((await Mediator.Send(command)).Result);
        }

        [Authorize(Roles = Roles.JobSeekerOrAdmin)]
        [HttpDelete("deletecv/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteCvCommand { Id = id, RequestedBy = CurrentUserId })).Result);

        [Authorize(Roles = Roles.JobSeeker)]
        [HttpPost("uploadfile")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile(IFormFileCollection files)
        {
            var command = new UploadCvFileCommand
            {
                JobSeekerId = CurrentUserId,
                Files = files.Select(file => new FileUploadRequest(
                    file.FileName, file.ContentType, file.Length, file.OpenReadStream())).ToList()
            };

            return Created((string?)null, (await Mediator.Send(command)).Result);
        }

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
