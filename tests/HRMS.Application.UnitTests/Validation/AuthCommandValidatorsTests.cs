using Application.Features.Auth.Commands;
using Application.Validation;

namespace HRMS.Application.UnitTests.Validation;

public class AuthCommandValidatorsTests
{
    private const string TooShort = "abcd";
    private const string JustLong = "abcde";
    private const string Valid = "Passw0rd!23";

    [Theory]
    [InlineData("", false)]
    [InlineData(TooShort, false)]
    [InlineData(JustLong, true)]
    [InlineData("55555", true)]
    [InlineData("aaaaa", true)]
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

    [Fact]
    public void ChangePassword_Should_RejectAnEmptyUserId()
        => new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand
        {
            UserId = Guid.Empty,
            CurrentPassword = Valid,
            NewPassword = Valid
        }).IsValid.ShouldBeFalse();

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("12345678901", true)]
    [InlineData("1234567890", false)]
    [InlineData("123456789012", false)]
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

    [Fact]
    public void RefreshToken_Should_NotJudgeWhetherTheTokenIsKnown()
        => new RefreshTokenCommandValidator()
            .Validate(new RefreshTokenCommand { RefreshToken = "completely-made-up" })
            .IsValid.ShouldBeTrue();
}
