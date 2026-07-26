using Application.Features.Auth.Commands;
using Application.Validation;

namespace HRMS.Application.UnitTests.Validation;

/// <summary>
/// Sign-in is the one request where what the validator does <i>not</i> check matters as much as what
/// it does, so both directions are pinned here.
/// </summary>
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

    /// <summary>
    /// Deliberate: no minimum length on sign-in. Rejecting a short password before verifying it
    /// would disclose the policy to an anonymous caller and lock out any account whose password
    /// predates the rule. Length belongs on registration and password change, not here.
    /// </summary>
    [Fact]
    public void Validate_Should_Pass_When_ThePasswordIsShort()
        => _sut.Validate(Command("altan@example.com", "x")).IsValid.ShouldBeTrue();

    /// <summary>
    /// The failure must depend only on the shape of the request. If a rule could distinguish two
    /// well-formed submissions, the 400/401 split would become a user-enumeration oracle — the exact
    /// leak the uniform 401 in AuthManager exists to close.
    /// </summary>
    [Fact]
    public void Validate_Should_Pass_Identically_For_KnownAndUnknownAccounts()
    {
        var known = _sut.Validate(Command("admin@hrms.local", "Passw0rd!23"));
        var unknown = _sut.Validate(Command("nobody@nowhere.invalid", "Passw0rd!23"));

        known.IsValid.ShouldBeTrue();
        unknown.IsValid.ShouldBe(known.IsValid);
    }
}
