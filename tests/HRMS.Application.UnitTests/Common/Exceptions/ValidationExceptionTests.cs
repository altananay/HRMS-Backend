using FluentValidation.Results;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace HRMS.Application.UnitTests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_Should_ProduceEmptyErrors_When_NoFailuresGiven()
    {
        new ValidationException([]).Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_Should_GroupMultipleMessagesUnderOneProperty()
    {
        var exception = new ValidationException([
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Email", "Email is not valid."),
            new ValidationFailure("Age", "Age must be positive.")
        ]);

        exception.Errors.Count.ShouldBe(2);
        exception.Errors["Email"].ShouldBe(["Email is required.", "Email is not valid."]);
        exception.Errors["Age"].ShouldBe(["Age must be positive."]);
    }

    [Fact]
    public void Constructor_Should_SetAGenericMessage()
    {
        // The per-field detail belongs in Errors; Message is what a caller sees as the title.
        new ValidationException([new ValidationFailure("Email", "required")])
            .Message.ShouldNotBeNullOrWhiteSpace();
    }
}
