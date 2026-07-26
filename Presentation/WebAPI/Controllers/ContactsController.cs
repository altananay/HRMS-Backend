using Application.Features.Contacts.Commands;
using Application.Features.Contacts.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class ContactsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllContactsQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdContactQuery { Id = id })).Result);

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Add(CreateContactCommand command)
        {
            var result = (await Mediator.Send(command)).Result;

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateContactCommand command)
        {
            command.Id = id;
            return Ok((await Mediator.Send(command)).Result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteContactCommand { Id = id })).Result);
    }
}
