using Application.Common.Exceptions;
using Application.Utilities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace WebAPI.Infrastructure
{
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
                _logger.LogError(exception, "Request {Method} {Path} failed with {StatusCode}",
                    httpContext.Request.Method, httpContext.Request.Path, problemDetails.Status);
            }
            else
            {
                _logger.LogWarning(
                    "Request {Method} {Path} rejected with {StatusCode} ({ExceptionType}): {Title}",
                    httpContext.Request.Method, httpContext.Request.Path,
                    problemDetails.Status, exception.GetType().Name, problemDetails.Title);
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
