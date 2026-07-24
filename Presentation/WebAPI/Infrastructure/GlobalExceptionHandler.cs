using Application.Common.Exceptions;
using Application.Utilities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace WebAPI.Infrastructure
{
    /// <summary>
    /// Maps application exceptions onto RFC 9457 ProblemDetails responses.
    /// </summary>
    /// <remarks>
    /// Replaces ConfigureExceptionHandlerExtension, which mapped every exception to HTTP 500 and
    /// echoed <c>Exception.Message</c> straight back to the caller. Two consequences of that:
    /// the API had effectively no 400/401/403/404 anywhere, and an unknown-email login — which threw
    /// BusinessException from JobSeekerBusinessRules — returned a 500 whose distinct message made it
    /// a user-enumeration oracle.
    ///
    /// Unhandled exceptions now surface only a traceId. The message is included in Development to
    /// keep debugging tolerable, and never outside it.
    /// </remarks>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _environment = environment;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = Map(exception, httpContext);

            if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            }
            else
            {
                _logger.LogInformation(
                    "Request {Method} {Path} rejected with {StatusCode}: {Title}",
                    httpContext.Request.Method, httpContext.Request.Path,
                    problemDetails.Status, problemDetails.Title);
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }

        private ProblemDetails Map(Exception exception, HttpContext httpContext)
        {
            switch (exception)
            {
                case ValidationException validationException:
                    return new ValidationProblemDetails(
                        validationException.Errors.ToDictionary(entry => entry.Key, entry => entry.Value))
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Doğrulama hatası.",
                        Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"
                    };

                case NotFoundException:
                    return Problem(StatusCodes.Status404NotFound, "Kayıt bulunamadı.", exception.Message);

                case ConflictException:
                    return Problem(StatusCodes.Status409Conflict, "Çakışma.", exception.Message);

                case ForbiddenException:
                    return Problem(StatusCodes.Status403Forbidden, "Yetkiniz yok.", exception.Message);

                case UnauthorizedAccessException:
                    return Problem(StatusCodes.Status401Unauthorized, "Kimlik doğrulanamadı.", detail: null);

                // BusinessException is thrown by the *BusinessRules classes for rule violations.
                // It is a bad request, not a server fault — this is the single biggest status-code
                // correction in the migration.
                case BusinessException:
                    return Problem(StatusCodes.Status400BadRequest, "İş kuralı ihlali.", exception.Message);

                default:
                    return Problem(
                        StatusCodes.Status500InternalServerError,
                        "Beklenmeyen bir hata oluştu.",
                        _environment.IsDevelopment() ? exception.ToString() : null);
            }
        }

        private static ProblemDetails Problem(int status, string title, string? detail) => new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
}
