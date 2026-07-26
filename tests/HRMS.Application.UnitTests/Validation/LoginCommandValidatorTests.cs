using Application.Features.Auth.Commands;
using Application.Validation;

namespace HRMS.Application.UnitTests.Validation;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    private static LoginCommand Command(string email, string password)
        => new() { Email = email, Password = password };

    [Theory]
    [InlineData("", "Passw0rd!23")]
    [InlineData("   ", "Passw0rd!23")]
    [InlineData("not-an-email", "Passw0rd!23")]
    [InlineData("altan@example.com", "")]
    [InlineData("", "")]
    public void Validate_Should_Fail_When_TheCredentialPairIsNotUsable(string email, string password)
        => _sut.Validate(Command(email, password)).IsValid.ShouldBeFalse();

    [Fact]
    public void Validate_Should_Pass_When_BothFieldsArePresentAndTheEmailIsWellFormed()
        => _sut.Validate(Command("altan@example.com", "Passw0rd!23")).IsValid.ShouldBeTrue();

    [Fact]
    public void Validate_Should_Pass_When_ThePasswordIsShort()
        => _sut.Validate(Command("altan@example.com", "x")).IsValid.ShouldBeTrue();

    [Fact]
    public void Validate_Should_Pass_Identically_For_KnownAndUnknownAccounts()
    {
        var known = _sut.Validate(Command("admin@hrms.local", "Passw0rd!23"));
        var unknown = _sut.Validate(Command("nobody@nowhere.invalid", "Passw0rd!23"));

        known.IsValid.ShouldBeTrue();
        unknown.IsValid.ShouldBe(known.IsValid);
    }
}
