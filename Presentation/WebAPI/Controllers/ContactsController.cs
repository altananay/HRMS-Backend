using Application.Features.Contacts.Commands;
using Application.Features.Contacts.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Application.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Contacts.Queries.GetAllContactsQuery;
using static Application.Features.Contacts.Queries.GetByIdContactQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class ContactsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContactsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            GetAllContactsQueryResponse response = await _mediator.Send(new GetAllContactsQuery { });
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpGet("/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            GetByIdContactQueryResponse response = await _mediator.Send(new GetByIdContactQuery { });
            if (response.Contact.IsSuccess)
            {
                return Ok(response.Contact);
            }
            return BadRequest(response.Contact);
        }

        [AllowAnonymous]

        [HttpPost]
        public async Task<IActionResult> Add(CreateContactCommand request)
        {
            var response = await _mediator.Send(request);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpDelete("/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _mediator.Send(new DeleteContactCommand { Id = id });
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("/{id}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateContactCommand request)
        {
            var response = await _mediator.Send(request);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }
    }
}