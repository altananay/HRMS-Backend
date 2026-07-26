using Application.Features.Auth.Commands;
using Application.Validation;

namespace HRMS.Application.UnitTests.Validation;

/// <summary>
/// The remaining auth commands. <see cref="LoginCommandValidatorTests"/> covers sign-in.
/// </summary>
public class AuthCommandValidatorsTests
{
    private const string TooShort = "abcd";      // 4 — one under the minimum
    private const string JustLong = "abcde";     // 5 — exactly the minimum
    private const string Valid = "Passw0rd!23";

    // ---------------------------------------------------------------------------------------------
    // Password policy — length only, and only where a password is being set
    // ---------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("", false)]
    [InlineData(TooShort, false)]
    [InlineData(JustLong, true)]
    [InlineData("55555", true)]          // all digits
    [InlineData("aaaaa", true)]          // all lowercase, no digit, no symbol
    [InlineData(Valid, true)]
    public void RegisterJobSeeker_Should_RequireLengthOnly(string password, bool expected)
    {
        var result = new RegisterJobSeekerCommandValidator().Validate(new RegisterJobSeekerCommand
        {
            Email = "seeker@example.com",
            Password = password,
            FirstName = "Altan",
            LastName = "Anay"
        });

        result.IsValid.ShouldBe(expected);
    }

    [Fact]
    public void RegisterEmployer_Should_RejectAPasswordUnderTheMinimum()
        => new RegisterEmployerCommandValidator().Validate(new RegisterEmployerCommand
        {
            Email = "hr@acme.com",
            Password = TooShort,
            CompanyName = "Acme"
        }).IsValid.ShouldBeFalse();

    [Fact]
    public void RegisterSystemStaff_Should_RejectAPasswordUnderTheMinimum()
        => new RegisterSystemStaffCommandValidator().Validate(new RegisterSystemStaffCommand
        {
            Email = "staff@hrms.local",
            Password = TooShort,
            FirstName = "Altan",
            LastName = "Anay"
        }).IsValid.ShouldBeFalse();

    /// <summary>
    /// The asymmetry that matters: the minimum guards a password being <i>set</i>, never one being
    /// <i>checked</i>. Applying it to CurrentPassword would lock out any account whose password
    /// predates the policy — they could no longer even change it.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_EnforceTheMinimumOnTheNewPasswordOnly()
    {
        var sut = new ChangePasswordCommandValidator();

        Command(current: TooShort, next: Valid).ShouldBeTrue();
        Command(current: Valid, next: TooShort).ShouldBeFalse();
        Command(current: "", next: Valid).ShouldBeFalse();

        bool Command(string current, string next) => sut.Validate(new ChangePasswordCommand
        {
            UserId = Guid.CreateVersion7(),
            CurrentPassword = current,
            NewPassword = next
        }).IsValid;
    }

    /// <summary>UserId is taken from the token; an empty one means an unauthenticated caller.</summary>
    [Fact]
    public void ChangePassword_Should_RejectAnEmptyUserId()
        => new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand
        {
            UserId = Guid.Empty,
            CurrentPassword = Valid,
            NewPassword = Valid
        }).IsValid.ShouldBeFalse();

    // ---------------------------------------------------------------------------------------------
    // National id — optional, but eleven digits when supplied
    // ---------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("12345678901", true)]
    [InlineData("1234567890", false)]     // ten
    [InlineData("123456789012", false)]   // twelve
    [InlineData("1234567890a", false)]
    public void RegisterJobSeeker_Should_AcceptNoNationalId_ButValidateTheShapeWhenGiven(
        string? nationalId, bool expected)
    {
        var result = new RegisterJobSeekerCommandValidator().Validate(new RegisterJobSeekerCommand
        {
            Email = "seeker@example.com",
            Password = Valid,
            FirstName = "Altan",
            LastName = "Anay",
            NationalId = nationalId
        });

        result.IsValid.ShouldBe(expected);
    }

    // ---------------------------------------------------------------------------------------------
    // Token commands — presence only
    // ---------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("fWMy3X9R-_8D3MgP1Ldm2u57J4ixIoUkSgKQXMDfQuQ", true)]
    public void RefreshToken_Should_RequirePresenceOnly(string token, bool expected)
    {
        new RefreshTokenCommandValidator()
            .Validate(new RefreshTokenCommand { RefreshToken = token }).IsValid.ShouldBe(expected);

        new LogoutCommandValidator()
            .Validate(new LogoutCommand { RefreshToken = token }).IsValid.ShouldBe(expected);
    }

    /// <summary>
    /// A refresh token that is well-formed but unknown, rotated or expired must not fail here —
    /// those are decided by the manager, and all of them have to look the same from outside.
    /// </summary>
    [Fact]
    public void RefreshToken_Should_NotJudgeWhetherTheTokenIsKnown()
        => new RefreshTokenCommandValidator()
            .Validate(new RefreshTokenCommand { RefreshToken = "completely-made-up" })
            .IsValid.ShouldBeTrue();
}
