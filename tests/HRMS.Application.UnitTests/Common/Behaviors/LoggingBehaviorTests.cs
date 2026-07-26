using Application.Abstractions;
using Application.Common.Behaviors;
using Application.Common.Exceptions;
using Application.Utilities.Exceptions;
using FluentValidation.Results;
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

    public static TheoryData<Exception> Failures() =>
    [
        new ValidationException([new ValidationFailure("Email", "E-posta zorunludur.")]),
        new NotFoundException("Kayıt bulunamadı."),
        new ConflictException("Bu ilana zaten başvurdunuz."),
        new ForbiddenException(),
        new BusinessException("İş kuralı ihlali."),
        new UnauthorizedAccessException("E-posta veya parola hatalı."),
        new InvalidOperationException("a genuine fault")
    ];

    /// <summary>
    /// A failed request is logged once, by GlobalExceptionHandler, which is the only place that
    /// knows the resulting status code. This behavior must stay silent or every rejection produces
    /// two entries — the duplication this test exists to prevent.
    /// </summary>
    [Theory]
    [MemberData(nameof(Failures))]
    public async Task Handle_Should_LogNothing_When_HandlerThrows(Exception failure)
    {
        _currentUser.UserId.Returns("user-1");

        await Should.ThrowAsync<Exception>(() => CreateSut().Handle(
            new SampleQuery("developer"),
            _ => Task.FromException<string>(failure),
            CancellationToken.None));

        LoggedLevels().ShouldBeEmpty();
    }

    /// <summary>
    /// The replaced LogAspect could not do this. Castle's MethodInterception is synchronous, so for
    /// an <c>async Task</c> method its OnSuccess/OnAfter hooks fired when the Task was *returned*,
    /// meaning it reported success for operations that went on to fail. Here the success line is
    /// written after the await completes, so a failure thrown later can never be logged as success.
    /// </summary>
    [Fact]
    public async Task Handle_Should_NotLogSuccess_When_HandlerFailsAsynchronously()
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

        LoggedLevels().ShouldBeEmpty();
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
