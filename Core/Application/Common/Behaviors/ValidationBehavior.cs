using FluentValidation;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.Common.Behaviors
{
    /// <summary>
    /// Runs every registered <see cref="IValidator{T}"/> for the incoming request before the handler.
    /// </summary>
    /// <remarks>
    /// This single behavior replaces all three previously overlapping validation mechanisms:
    /// FluentValidation.AspNetCore auto-validation, the globally registered MVC ValidationFilter, and
    /// the 46 <c>[ValidationAspect(typeof(X))]</c> Castle attributes on manager methods.
    ///
    /// The aspect it replaces validated by reflecting over <c>invocation.Arguments</c> and selecting
    /// those whose runtime type matched the validator's generic argument. That silently validated
    /// nothing whenever the argument was a plain <c>string</c> — which is why all 21
    /// <c>ObjectIdValidator</c> attributes never actually ran. Validating the request object itself
    /// removes that whole class of failure, and unlike the aspect it is properly asynchronous.
    /// </remarks>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var failures = results.SelectMany(result => result.Errors).Where(failure => failure is not null).ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
