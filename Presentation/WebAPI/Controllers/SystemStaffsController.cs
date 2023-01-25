using Application.Features.JobSeekers.Queries;
using Application.Features.SystemStaffs.Commands;
using Application.Features.SystemStaffs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.SystemStaffs.Commands.CreateSystemStaffCommand;
using static Application.Features.SystemStaffs.Commands.DeleteSystemStaffCommand;
using static Application.Features.SystemStaffs.Commands.UpdateSystemStaffCommand;
using static Application.Features.SystemStaffs.Queries.GetAllSystemStaffQuery;
using static Application.Features.SystemStaffs.Queries.GetByIdSystemStaffQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemStaffsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SystemStaffsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllSystemStaffQueryResponse response = await _mediator.Send(new GetAllSystemStaffQuery { });
            if (response.SystemStaffs.IsSuccess)
            {
                return Ok(response.SystemStaffs);
            }
            return BadRequest(response.SystemStaffs);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(CreateSystemStaffCommand systemStaff)
        {
            CreateSystemStaffCommandResponse response = await _mediator.Send(systemStaff);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteSystemStaffCommandResponse response = await _mediator.Send(new DeleteSystemStaffCommand { Id = id});
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateSystemStaffCommand systemStaff)
        {
            UpdateSystemStaffCommandResponse response = await _mediator.Send(systemStaff);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            GetByIdSystemStaffQueryResponse response = await _mediator.Send(new GetByIdSystemStaffQuery { Id = id});
            if (response.SystemStaff.IsSuccess)
            {
                return Ok(response.SystemStaff);
            }
            return BadRequest(response.SystemStaff);
        }
    }
}