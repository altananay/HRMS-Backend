using System.Diagnostics;
using Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors
{
    /// <summary>
    /// Logs one structured event per request: name, calling user, elapsed time, and outcome.
    /// </summary>
    /// <remarks>
    /// Replaces LogAspect, which had six constructor overloads, resolved <c>ILogger</c> from the
    /// static ServiceTool locator, logged <c>onBeforeMessage</c> from its <c>OnAfter</c> hook, and —
    /// because Castle's MethodInterception is synchronous over <c>async Task</c> methods — reported
    /// "success" the moment a Task was returned rather than when it completed. Its parameterless
    /// form, the one actually applied to both Login methods, emitted the constant strings
    /// "on before log" / "on after log", so authentication had no usable audit trail at all.
    ///
    /// Request objects are never logged wholesale: commands carry passwords, national IDs and
    /// emails. Only the request type name and the authenticated user id are recorded.
    /// </remarks>
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUser;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger,
            ICurrentUserService currentUser)
        {
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUser.UserId;
            var stopwatch = Stopwatch.StartNew();

            // No try/catch, deliberately: failures are logged once, by GlobalExceptionHandler.
            // Logging them here as well produced two entries for every rejected request — a Warning
            // from this behavior and an Information from the handler — which is what "one failed
            // login, two log lines" looked like in Seq. The handler is the better of the two places:
            // it sits outermost, so it also sees exceptions that never reach a MediatR handler, and
            // it knows the resulting status code, which is the field you actually filter on.
            //
            // Nothing is lost by letting the exception pass through untouched: if next() throws, the
            // success line below is never written, which is exactly the signal it should carry.
            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation(
                "Request {RequestName} handled for user {UserId} in {ElapsedMilliseconds}ms",
                requestName, userId, stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}
