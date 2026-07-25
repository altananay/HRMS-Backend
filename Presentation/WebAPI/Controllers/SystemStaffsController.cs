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

        // Staff creation moves to POST /api/auth/register/system-staff in Phase 4.
        //
        // It cannot live here as a plain CRUD add: creating a staff member means creating a User
        // with a password hash and a role assignment. The old POST add went through
        // SystemStaffAuthManager.Register, which carried [ValidationAspect(typeof(
        // CreateSystemStaffCommand))] — a MediatR command passed where an IValidator was expected,
        // so ValidationAspect's constructor threw before the method could run at all.
    }
}
