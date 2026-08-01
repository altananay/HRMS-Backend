using System.Net;

namespace HRMS.WebAPI.FunctionalTests;

[Collection(ApiCollection.Name)]
public class PasswordResetTests : IAsyncLifetime
{
    private const string Email = "reset@test.local";
    private const string Password = "Passw0rd!23";
    private const string NewPassword = "BrandNew!Pass456";

    private readonly HrmsApiFactory _factory;
    private HrmsClient _client = null!;

    public PasswordResetTests(HrmsApiFactory factory) => _factory = factory;

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _factory.Mail.Clear();
        _client = new HrmsClient(_factory.CreateClient());

        (await _client.RegisterJobSeekerAsync(Email, Password)).IsSuccess.ShouldBeTrue();
        _client.Authenticate(null);
        _factory.Mail.Clear();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private Task<HrmsClient.Response> RequestResetAsync(string email)
        => _client.PostAsync("/api/auth/forgot-password", new { email });

    private static string StripTraceId(string body)
        => System.Text.RegularExpressions.Regex.Replace(body, "\"traceId\":\"[^\"]*\"", string.Empty);

    [Fact]
    public async Task ForgotPassword_Should_AnswerIdenticallyForKnownAndUnknownAddresses()
    {
        var known = await RequestResetAsync(Email);
        var unknown = await RequestResetAsync("nobody@test.local");

        known.Status.ShouldBe(HttpStatusCode.OK, known.Body);
        unknown.Status.ShouldBe(HttpStatusCode.OK, unknown.Body);
        StripTraceId(known.Body).ShouldBe(StripTraceId(unknown.Body));
    }

    [Fact]
    public async Task ForgotPassword_Should_MailOnlyTheRegisteredAddress()
    {
        await RequestResetAsync(Email);
        await RequestResetAsync("nobody@test.local");

        _factory.Mail.ResetTokenFor(Email).ShouldNotBeNullOrWhiteSpace();
        _factory.Mail.LastTo("nobody@test.local").ShouldBeNull();
    }

    [Fact]
    public async Task ForgotPassword_Should_SendALinkToTheConfiguredResetScreen()
    {
        await RequestResetAsync(Email);

        var mail = _factory.Mail.LastTo(Email);

        mail.ShouldNotBeNull();
        mail.TextBody.ShouldContain("http://localhost:3000/reset-password?token=");
        mail.HtmlBody.ShouldContain("http://localhost:3000/reset-password?token=");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task ForgotPassword_Should_Return400_When_TheAddressIsUnusable(string email)
        => (await RequestResetAsync(email)).Status.ShouldBe(HttpStatusCode.BadRequest);

    [Fact]
    public async Task RequestingASecondLink_Should_RetireTheFirst()
    {
        await RequestResetAsync(Email);
        var first = _factory.Mail.ResetTokenFor(Email)!;

        await RequestResetAsync(Email);
        var second = _factory.Mail.ResetTokenFor(Email)!;

        second.ShouldNotBe(first);

        (await _client.PostAsync("/api/auth/reset-password", new { token = first, newPassword = NewPassword }))
            .Status.ShouldBe(HttpStatusCode.BadRequest);

        (await _client.PostAsync("/api/auth/reset-password", new { token = second, newPassword = NewPassword }))
            .IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ResetPassword_Should_ReplaceThePassword()
    {
        await RequestResetAsync(Email);
        var token = _factory.Mail.ResetTokenFor(Email)!;

        var reset = await _client.PostAsync("/api/auth/reset-password", new { token, newPassword = NewPassword });
        reset.IsSuccess.ShouldBeTrue(reset.Body);

        (await _client.LoginAsync(Email, NewPassword)).Status.ShouldBe(HttpStatusCode.OK);
        (await _client.LoginAsync(Email, Password)).Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_Should_EndEveryExistingSession()
    {
        var registration = await _client.LoginAsync(Email, Password);
        var accessToken = registration.DataString("accessToken");
        var refreshToken = registration.DataString("refreshToken");

        _client.Authenticate(accessToken);
        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.OK);

        _client.Authenticate(null);
        await RequestResetAsync(Email);
        var token = _factory.Mail.ResetTokenFor(Email)!;
        (await _client.PostAsync("/api/auth/reset-password", new { token, newPassword = NewPassword }))
            .IsSuccess.ShouldBeTrue();

        _client.Authenticate(accessToken);
        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.Unauthorized);

        _client.Authenticate(null);
        (await _client.PostAsync("/api/auth/refresh", new { refreshToken }))
            .Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_Should_RefuseAReusedToken()
    {
        await RequestResetAsync(Email);
        var token = _factory.Mail.ResetTokenFor(Email)!;

        (await _client.PostAsync("/api/auth/reset-password", new { token, newPassword = NewPassword }))
            .IsSuccess.ShouldBeTrue();

        (await _client.PostAsync("/api/auth/reset-password", new { token, newPassword = "AnotherOne!99" }))
            .Status.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_RefuseAnUnknownToken()
        => (await _client.PostAsync("/api/auth/reset-password", new
        {
            token = "completely-made-up-token",
            newPassword = NewPassword
        })).Status.ShouldBe(HttpStatusCode.BadRequest);

    [Theory]
    [InlineData("", "BrandNew!Pass456")]
    [InlineData("some-token", "")]
    [InlineData("some-token", "abcd")]
    public async Task ResetPassword_Should_Return400_When_TheRequestIsUnusable(string token, string newPassword)
        => (await _client.PostAsync("/api/auth/reset-password", new { token, newPassword }))
            .Status.ShouldBe(HttpStatusCode.BadRequest);
}
