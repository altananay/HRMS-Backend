using Application.Features.SystemStaffs.Commands;
using Application.Features.SystemStaffs.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class SystemStaffsController : ApiControllerBase
    {
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllSystemStaffQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdSystemStaffQuery { Id = id })).Result);

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateSystemStaffCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [HttpDelete("deletebyid/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteSystemStaffCommand { Id = id })).Result);

    }
}
