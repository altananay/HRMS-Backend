using Application.Features.Contacts.Commands;
using Application.Features.Contacts.Queries;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <remarks>
    /// The routes here were previously <c>[HttpGet("/{id}")]</c>, <c>[HttpPut("/{id}")]</c> and
    /// <c>[HttpDelete("/{id}")]</c> — a leading slash escapes the <c>api/[controller]</c> prefix, so
    /// those three actions were actually served from the application root. They are relative now and
    /// carry a <c>:guid</c> constraint, so a malformed id is rejected during routing instead of
    /// reaching a handler and throwing.
    ///
    /// Note what that rejection looks like in practice: a malformed id matches no endpoint at all,
    /// and because <c>[AllowAnonymous]</c> is endpoint metadata it cannot apply to a request that
    /// matched nothing — the authorization fallback policy challenges it, so an anonymous caller
    /// sees <b>401</b> rather than 404. A well-formed id that simply does not exist gets the proper
    /// 404 ProblemDetails from the handler.
    /// </remarks>
    [Authorize(Roles = Roles.Admin)]
    public class ContactsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllContactsQuery query)
            => Ok((await Mediator.Send(query)).Result);

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok((await Mediator.Send(new GetByIdContactQuery { Id = id })).Result);

        /// <summary>Public contact form.</summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Add(CreateContactCommand command)
            => Ok((await Mediator.Send(command)).Result);

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateContactCommand command)
        {
            // Route wins over body, so a mismatched id in the payload cannot redirect the write.
            command.Id = id;
            return Ok((await Mediator.Send(command)).Result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok((await Mediator.Send(new DeleteContactCommand { Id = id })).Result);
    }
}
