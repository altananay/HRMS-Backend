using Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace HRMS.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record SampleCommand(string Email, int Age) : IRequest<string>;

    private sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(command => command.Email).NotEmpty().EmailAddress();
            RuleFor(command => command.Age).GreaterThan(0);
        }
    }

    private static ValidationBehavior<SampleCommand, string> CreateSut(params IValidator<SampleCommand>[] validators)
        => new(validators);

    [Fact]
    public async Task Handle_Should_InvokeNext_When_NoValidatorsAreRegistered()
    {
        var sut = CreateSut();
        var nextWasCalled = false;

        var result = await sut.Handle(
            new SampleCommand("not-an-email", -1),
            _ => { nextWasCalled = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        nextWasCalled.ShouldBeTrue();
        result.ShouldBe("ok");
    }

    [Fact]
    public async Task Handle_Should_InvokeNext_When_RequestIsValid()
    {
        var sut = CreateSut(new SampleCommandValidator());

        var result = await sut.Handle(
            new SampleCommand("altan@example.com", 30),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        result.ShouldBe("ok");
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_When_RequestIsInvalid()
    {
        var sut = CreateSut(new SampleCommandValidator());

        await Should.ThrowAsync<ValidationException>(() => sut.Handle(
            new SampleCommand("not-an-email", -1),
            _ => Task.FromResult("ok"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_NotInvokeNext_When_ValidationFails()
    {
        var sut = CreateSut(new SampleCommandValidator());
        var nextWasCalled = false;

        await Should.ThrowAsync<ValidationException>(() => sut.Handle(
            new SampleCommand("", 0),
            _ => { nextWasCalled = true; return Task.FromResult("ok"); },
            CancellationToken.None));

        // The whole point of validating in the pipeline: the handler must never see a bad request.
        nextWasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_Should_GroupFailuresByPropertyName()
    {
        var sut = CreateSut(new SampleCommandValidator());

        var exception = await Should.ThrowAsync<ValidationException>(() => sut.Handle(
            new SampleCommand("", 0),
            _ => Task.FromResult("ok"),
            CancellationToken.None));

        exception.Errors.ShouldContainKey(nameof(SampleCommand.Email));
        exception.Errors.ShouldContainKey(nameof(SampleCommand.Age));
        exception.Errors[nameof(SampleCommand.Age)].ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_AggregateFailuresAcrossAllValidators()
    {
        var sut = CreateSut(new SampleCommandValidator(), new SecondSampleCommandValidator());

        var exception = await Should.ThrowAsync<ValidationException>(() => sut.Handle(
            new SampleCommand("", 0),
            _ => Task.FromResult("ok"),
            CancellationToken.None));

        // Every registered IValidator<T> runs, not just the first match.
        exception.Errors.ShouldContainKey(nameof(SampleCommand.Email));
        exception.Errors[nameof(SampleCommand.Email)].Length.ShouldBeGreaterThan(1);
    }

    private sealed class SecondSampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SecondSampleCommandValidator()
            => RuleFor(command => command.Email).MinimumLength(5).WithMessage("Email is too short.");
    }

    /// <summary>
    /// Regression guard for the aspect this behavior replaced. ValidationAspect selected arguments
    /// via <c>invocation.Arguments.Where(t =&gt; t.GetType() == entityType)</c>, so a manager method
    /// taking a plain <c>string id</c> validated nothing at all â€” which is why all 21
    /// <c>[ValidationAspect(typeof(ObjectIdValidator))]</c> attributes were silently inert.
    /// </summary>
    [Fact]
    public async Task Handle_Should_ValidateRequestsWithPrimitiveMembers()
    {
        var sut = new ValidationBehavior<PrimitiveCommand, string>([new PrimitiveCommandValidator()]);

        await Should.ThrowAsync<ValidationException>(() => sut.Handle(
            new PrimitiveCommand(""),
            _ => Task.FromResult("ok"),
            CancellationToken.None));
    }

    public sealed record PrimitiveCommand(string Id) : IRequest<string>;

    private sealed class PrimitiveCommandValidator : AbstractValidator<PrimitiveCommand>
    {
        public PrimitiveCommandValidator() => RuleFor(command => command.Id).NotEmpty();
    }
}
