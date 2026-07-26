using FluentValidation.Results;

namespace Application.Common.Exceptions
{
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
