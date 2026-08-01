using System.Diagnostics;
using Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors
{
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

            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation(
                "Request {RequestName} handled for user {UserId} in {ElapsedMilliseconds}ms",
                requestName, userId, stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}
