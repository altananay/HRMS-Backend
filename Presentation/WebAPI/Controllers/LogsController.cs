using Application.Features.Logs.Queries;
using Application.Features.Logs.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Logs.Queries.GetAllErrorLogsQuery;
using static Application.Features.Logs.Queries.GetAllInfosLogsQuery;
using static Application.Features.Logs.Query.GetAllLogsQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            GetAllQueryLogsResponse result = await _mediator.Send(new GetAllLogsQuery { });
            if (result.Results.IsSuccess)
            {
                return Ok(result.Results);
            }
            return BadRequest(result.Results);
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors()
        {
            GetAllErrorLogsQueryResponse result = await _mediator.Send(new GetAllErrorLogsQuery { });
            if (result.Results.IsSuccess) { return Ok(result.Results); }
            return BadRequest(result.Results);
        }

        [HttpGet("infos")]
        public async Task<IActionResult> GetInfos()
        {
            GetAllInfosLogsQueryResponse result = await _mediator.Send(new GetAllInfosLogsQuery { });
            if (result.Results.IsSuccess) { return Ok(result.Results); }
            return BadRequest(result.Results);
        }
    }
}