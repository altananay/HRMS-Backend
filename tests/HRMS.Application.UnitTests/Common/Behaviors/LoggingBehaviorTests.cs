using Application.Abstractions;
using Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.UnitTests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record SampleQuery(string Term) : IRequest<string>;

    private readonly ILogger<LoggingBehavior<SampleQuery, string>> _logger =
        Substitute.For<ILogger<LoggingBehavior<SampleQuery, string>>>();

    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    private LoggingBehavior<SampleQuery, string> CreateSut() => new(_logger, _currentUser);

    /// <summary>
    /// The levels passed to <see cref="ILogger.Log{TState}"/>, read off the recorded calls.
    /// </summary>
    /// <remarks>
    /// Asserting via <c>ReceivedWithAnyArgs().Log(LogLevel.X, ...)</c> does not work: "with any
    /// args" ignores every argument including the level, so it matches a log call at any level.
    /// The generic TState is the internal FormattedLogValues type, which also makes an
    /// <c>Arg.Any&lt;object&gt;()</c> overload fail to bind — so inspect the calls directly.
    /// </remarks>
    private IReadOnlyList<LogLevel> LoggedLevels() =>
        _logger.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
            .Select(call => (LogLevel)call.GetArguments()[0]!)
            .ToList();

    [Fact]
    public async Task Handle_Should_ReturnHandlerResponse()
    {
        _currentUser.UserId.Returns("user-1");

        var result = await CreateSut().Handle(
            new SampleQuery("developer"),
            _ => Task.FromResult("payload"),
            CancellationToken.None);

        result.ShouldBe("payload");
    }

    [Fact]
    public async Task Handle_Should_LogInformation_When_RequestSucceeds()
    {
        _currentUser.UserId.Returns("user-1");

        await CreateSut().Handle(
            new SampleQuery("developer"),
            _ => Task.FromResult("payload"),
            CancellationToken.None);

        LoggedLevels().ShouldBe([LogLevel.Information]);
    }

    [Fact]
    public async Task Handle_Should_RethrowException_When_HandlerThrows()
    {
        var boom = new InvalidOperationException("boom");

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().Handle(
            new SampleQuery("developer"),
            _ => Task.FromException<string>(boom),
            CancellationToken.None));

        thrown.ShouldBeSameAs(boom);
    }

    /// <summary>
    /// The replaced LogAspect could not do this. Castle's MethodInterception is synchronous, so for
    /// an <c>async Task</c> method its OnSuccess/OnAfter hooks fired when the Task was *returned*
    /// and OnException never observed a failure thrown after the first await â€” meaning it reported
    /// success for operations that went on to fail.
    /// </summary>
    [Fact]
    public async Task Handle_Should_LogError_When_HandlerFailsAsynchronously()
    {
        _currentUser.UserId.Returns("user-1");

        await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().Handle(
            new SampleQuery("developer"),
            async _ =>
            {
                await Task.Yield();
                throw new InvalidOperationException("failed after await");
            },
            CancellationToken.None));

        // Exactly one Error and no success line — the failure is observed after the await, which is
        // precisely what the synchronous Castle interceptor could not do.
        LoggedLevels().ShouldBe([LogLevel.Error]);
    }

    [Fact]
    public async Task Handle_Should_NotThrow_When_RequestIsAnonymous()
    {
        _currentUser.UserId.Returns((string?)null);

        var result = await CreateSut().Handle(
            new SampleQuery("developer"),
            _ => Task.FromResult("payload"),
            CancellationToken.None);

        result.ShouldBe("payload");
    }
}
