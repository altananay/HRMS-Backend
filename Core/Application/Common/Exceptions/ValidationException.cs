using FluentValidation.Results;

namespace Application.Common.Exceptions
{
    /// <summary>
    /// Aggregated FluentValidation failures raised by <see cref="Behaviors.ValidationBehavior{TRequest,TResponse}"/>.
    /// Mapped to HTTP 400 with a per-field error dictionary by GlobalExceptionHandler.
    /// </summary>
    /// <remarks>
    /// Deliberately distinct from <c>FluentValidation.ValidationException</c>: this one carries the
    /// failures already grouped by property name, which is the shape ValidationProblemDetails wants.
    /// </remarks>
    public class ValidationException : Exception
    {
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("Bir veya daha fazla doğrulama hatası oluştu.")
        {
            Errors = failures
                .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage)
                .ToDictionary(group => group.Key, group => group.ToArray());
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
