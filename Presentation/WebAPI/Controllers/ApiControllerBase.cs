using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected Guid CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(value, out var id)
                    ? id
                    : throw new ForbiddenException("Kimlik bilgisi okunamadı.");
            }
        }

        protected IActionResult Ok(Application.Results.IResult result) => base.Ok(result);
    }
}
