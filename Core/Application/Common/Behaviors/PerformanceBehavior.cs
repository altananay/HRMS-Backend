using System.Diagnostics;
using Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors
{
    /// <summary>
    /// Warns when a request exceeds <see cref="SlowRequestThresholdMilliseconds"/>.
    /// </summary>
    /// <remarks>
    /// Cheap early-warning for the N+1 queries and missing indexes that the move to a relational
    /// store makes possible. Unlike LoggingBehavior this stays quiet on the happy path.
    /// </remarks>
    public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private const long SlowRequestThresholdMilliseconds = 500;

        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUser;

        public PerformanceBehavior(
            ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
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
            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMilliseconds)
            {
                _logger.LogWarning(
                    "Slow request: {RequestName} took {ElapsedMilliseconds}ms for user {UserId}",
                    typeof(TRequest).Name, stopwatch.ElapsedMilliseconds, _currentUser.UserId);
            }

            return response;
        }
    }
}
