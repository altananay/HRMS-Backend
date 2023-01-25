using Application.Features.Cvs.Commands;
using Application.Features.Cvs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Cvs.Commands.CreateCvCommand;
using static Application.Features.Cvs.Commands.DeleteCvCommand;
using static Application.Features.Cvs.Commands.UpdateCvCommand;
using static Application.Features.Cvs.Queries.GetAllCvQuery;
using static Application.Features.Cvs.Queries.GetByJobSeekerIdCvQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CvsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CvsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllCvQueryResponse response = await _mediator.Send(new GetAllCvQuery { });
            if (response.Cvs.IsSuccess)
            {
                return Ok(response.Cvs);
            }
            return BadRequest(response.Cvs);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateCvCommand cv)
        {
            CreateCvCommandResponse response = await _mediator.Send(cv);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateCvCommand cv)
        {
            UpdateCvCommandResponse response = await _mediator.Send(cv);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpDelete("deletecv/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteCvCommandResponse response = await _mediator.Send(new DeleteCvCommand { Id = id});
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpGet("getbyjobseekerid/{JobSeekerId}")]
        public async Task<IActionResult> GetById([FromRoute] GetByJobSeekerIdCvQuery id)
        {
            GetByJobSeekerIdCvQueryResponse response = await _mediator.Send(id);
            if (response.Cv.IsSuccess)
            {
                return Ok(response.Cv);
            }
            return BadRequest(response.Cv);
        }

        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile([FromQuery] UploadCvFileCommand cvFile)
        {
            await _mediator.Send(cvFile);
            return Ok();
        }
    }
}