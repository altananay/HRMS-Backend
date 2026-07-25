using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Shared plumbing for the API controllers.
    /// </summary>
    /// <remarks>
    /// Every controller previously repeated
    /// <c>if (response.X.IsSuccess) return Ok(...); return BadRequest(...);</c> against a
    /// differently-named response member per module. That branch was also unreachable on read
    /// paths, because managers returned <c>SuccessDataResult</c> unconditionally and signalled
    /// failure by throwing. With failures now travelling as exceptions to ProblemDetails, reaching
    /// the controller at all means success — so <see cref="Ok(IResult)"/> is the whole story.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        /// <summary>The authenticated caller's id, or throws if the endpoint is not protected.</summary>
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

        // Fully qualified: WebAPI's implicit usings pull in Microsoft.AspNetCore.Http.IResult, which
        // collides with the Result-pattern IResult the managers return.
        protected IActionResult Ok(Application.Results.IResult result) => base.Ok(result);
    }
}
