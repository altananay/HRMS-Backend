using Application.Features.JobAdvertisements.Commands;
using Application.Features.JobAdvertisements.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.JobAdvertisements.Commands.CreateJobAdvertisementCommand;
using static Application.Features.JobAdvertisements.Commands.DeleteJobAdvertisementCommand;
using static Application.Features.JobAdvertisements.Commands.UpdateJobAdvertisementCommand;
using static Application.Features.JobAdvertisements.Queries.GetAllJobAdvertisementQuery;
using static Application.Features.JobAdvertisements.Queries.GetByIdJobAdvertisementQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobAdvertisementsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobAdvertisementsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllJobAdvertisementQueryResponse response = await _mediator.Send(new GetAllJobAdvertisementQuery { });
            if (response.JobAdvertisements.IsSuccess)
            {
                return Ok(response.JobAdvertisements);
            }
            return BadRequest(response.JobAdvertisements);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateJobAdvertisementCommand jobAdvertisement)
        {
            CreateJobAdvertisementCommandResponse response = await _mediator.Send(jobAdvertisement);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteJobAdvertisementCommandResponse response = await _mediator.Send(new DeleteJobAdvertisementCommand { Id = id });
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateJobAdvertisementCommand jobAdvertisement)
        {
            UpdateJobAdvertisementCommandResponse response = await _mediator.Send(jobAdvertisement);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            GetByIdJobAdvertisementQueryResponse response = await _mediator.Send(new GetByIdJobAdvertisementQuery { Id = id});
            if (response.JobAdvertisement.IsSuccess)
            {
                return Ok(response.JobAdvertisement);
            }
            return BadRequest(response.JobAdvertisement);
        }
    }
}