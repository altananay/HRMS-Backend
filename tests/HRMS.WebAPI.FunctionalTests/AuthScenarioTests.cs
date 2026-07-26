using System.Net;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// The token lifecycle, end to end over real HTTP against a real database.
/// </summary>
/// <remarks>
/// These assert the behaviours that were previously verified by hand. The two that matter most are
/// refresh-token reuse detection and security-stamp revocation: both are silent when they break —
/// tokens that should be dead simply keep working, and nothing fails loudly to tell you.
/// </remarks>
[Collection(ApiCollection.Name)]
public class AuthScenarioTests : IAsyncLifetime
{
    private readonly HrmsApiFactory _factory;
    private HrmsClient _client = null!;

    public AuthScenarioTests(HrmsApiFactory factory) => _factory = factory;

    public async ValueTask InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = new HrmsClient(_factory.CreateClient());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Register_Should_ReturnUsableTokens()
    {
        var response = await _client.RegisterJobSeekerAsync("seeker@test.local");

        response.IsSuccess.ShouldBeTrue(response.Body);
        response.DataString("accessToken").ShouldNotBeNullOrWhiteSpace();
        response.DataString("refreshToken").ShouldNotBeNullOrWhiteSpace();

        _client.Authenticate(response.DataString("accessToken"));
        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Me_Should_Return401_When_NoTokenIsPresented()
    {
        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Unknown email and wrong password must be indistinguishable.
    /// </summary>
    /// <remarks>
    /// Previously an unknown email escaped as a BusinessException and surfaced as a 500 carrying a
    /// distinct message, while a wrong password returned something else again — a reliable oracle
    /// for deciding which addresses are registered.
    /// </remarks>
    [Fact]
    public async Task Login_Should_BeIndistinguishable_For_UnknownEmailAndWrongPassword()
    {
        await _client.RegisterJobSeekerAsync("known@test.local");
        _client.Authenticate(null);

        var unknownEmail = await _client.LoginAsync("nobody@test.local", "Passw0rd!23");
        var wrongPassword = await _client.LoginAsync("known@test.local", "CompletelyWrong!9");

        unknownEmail.Status.ShouldBe(HttpStatusCode.Unauthorized);
        wrongPassword.Status.ShouldBe(HttpStatusCode.Unauthorized);

        // Same status and same body — traceId aside, nothing distinguishes the two.
        StripTraceId(unknownEmail.Body).ShouldBe(StripTraceId(wrongPassword.Body));
    }

    /// <summary>
    /// An unusable credential pair is a bad request, not a failed authentication.
    /// </summary>
    /// <remarks>
    /// There was no validator on LoginCommand, so blank fields reached the manager, missed on the
    /// lookup and came back 401 — claiming the credentials had been rejected when none were
    /// submitted and no verification ever ran.
    /// </remarks>
    [Theory]
    [InlineData("", "")]
    [InlineData("", "Passw0rd!23")]
    [InlineData("someone@test.local", "")]
    [InlineData("not-an-email", "Passw0rd!23")]
    public async Task Login_Should_Return400_When_TheRequestCannotBeEvaluated(string email, string password)
    {
        var response = await _client.LoginAsync(email, password);

        response.Status.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// The 400/401 split must follow the shape of the request, never the existence of the account —
    /// otherwise the new status code becomes the enumeration oracle that the uniform 401 closed.
    /// </summary>
    [Fact]
    public async Task Login_Should_Return401_ForBothKnownAndUnknownAccounts_When_TheRequestIsWellFormed()
    {
        await _client.RegisterJobSeekerAsync("shaped@test.local");
        _client.Authenticate(null);

        var known = await _client.LoginAsync("shaped@test.local", "CompletelyWrong!9");
        var unknown = await _client.LoginAsync("absent@test.local", "CompletelyWrong!9");

        known.Status.ShouldBe(HttpStatusCode.Unauthorized);
        unknown.Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_Should_IssueANewPair_And_RetireTheOldToken()
    {
        var registration = await _client.RegisterJobSeekerAsync("rotate@test.local");
        var originalRefresh = registration.DataString("refreshToken");

        var refreshed = await _client.PostAsync("/api/auth/refresh", new { refreshToken = originalRefresh });

        refreshed.IsSuccess.ShouldBeTrue(refreshed.Body);
        refreshed.DataString("refreshToken").ShouldNotBe(originalRefresh);
    }

    /// <summary>
    /// Replaying a rotated refresh token is treated as theft and kills every session.
    /// </summary>
    /// <remarks>
    /// This is the whole point of rotation. Revoking only the replayed token would leave whoever
    /// holds the newer one — quite possibly the attacker — with a working session.
    ///
    /// The final assertion is the one that was missing before token versioning: without it, the
    /// server concludes a token was stolen and then lets the thief keep making authenticated
    /// requests until the access token expires.
    /// </remarks>
    [Fact]
    public async Task ReplayingARotatedRefreshToken_Should_RevokeTheEntireChain()
    {
        var registration = await _client.RegisterJobSeekerAsync("theft@test.local");
        var accessToken = registration.DataString("accessToken");
        var firstRefresh = registration.DataString("refreshToken");

        var rotated = await _client.PostAsync("/api/auth/refresh", new { refreshToken = firstRefresh });
        var secondRefresh = rotated.DataString("refreshToken");

        // The attacker replays the token that was already rotated away.
        var replay = await _client.PostAsync("/api/auth/refresh", new { refreshToken = firstRefresh });
        replay.Status.ShouldBe(HttpStatusCode.Unauthorized);

        // The legitimate holder's newer refresh token is dead too — the chain went with it.
        var afterRevocation = await _client.PostAsync("/api/auth/refresh", new { refreshToken = secondRefresh });
        afterRevocation.Status.ShouldBe(HttpStatusCode.Unauthorized);

        // And the access token minted before the theft stops working immediately.
        _client.Authenticate(accessToken);
        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Changing a password ends existing sessions immediately, not when the token expires.
    /// </summary>
    [Fact]
    public async Task ChangingPassword_Should_InvalidateTheExistingAccessToken()
    {
        var registration = await _client.RegisterJobSeekerAsync("pwchange@test.local");
        var accessToken = registration.DataString("accessToken");
        _client.Authenticate(accessToken);

        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.OK);

        var changed = await _client.PostAsync("/api/auth/change-password", new
        {
            currentPassword = "Passw0rd!23",
            newPassword = "BrandNew!Pass456"
        });
        changed.IsSuccess.ShouldBeTrue(changed.Body);

        // Same token, now rejected — this is the security stamp being checked per request.
        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.Unauthorized);

        _client.Authenticate(null);
        (await _client.LoginAsync("pwchange@test.local", "BrandNew!Pass456")).Status.ShouldBe(HttpStatusCode.OK);
        (await _client.LoginAsync("pwchange@test.local", "Passw0rd!23")).Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutAll_Should_InvalidateTheExistingAccessToken()
    {
        var registration = await _client.RegisterJobSeekerAsync("logoutall@test.local");
        _client.Authenticate(registration.DataString("accessToken"));

        (await _client.PostAsync("/api/auth/logout-all")).IsSuccess.ShouldBeTrue();

        (await _client.GetAsync("/api/auth/me")).Status.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Should_RejectTheRefreshTokenAfterwards()
    {
        var registration = await _client.RegisterJobSeekerAsync("logout@test.local");
        var refreshToken = registration.DataString("refreshToken");

        (await _client.PostAsync("/api/auth/logout", new { refreshToken })).IsSuccess.ShouldBeTrue();

        (await _client.PostAsync("/api/auth/refresh", new { refreshToken })).Status
            .ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_Should_Return409_When_TheEmailIsAlreadyUsed()
    {
        await _client.RegisterJobSeekerAsync("duplicate@test.local");
        _client.Authenticate(null);

        (await _client.RegisterJobSeekerAsync("duplicate@test.local")).Status.ShouldBe(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// The seeded administrator can actually sign in.
    /// </summary>
    /// <remarks>
    /// Previously impossible: creating staff required <c>[SecuredOperation("admin")]</c> and so did
    /// the lookup used to log one in, so an admin was needed to create the first admin — the only
    /// escape was inserting a document straight into the database.
    /// </remarks>
    [Fact]
    public async Task SeededAdministrator_Should_BeAbleToSignIn()
    {
        var response = await _client.LoginAsync(HrmsApiFactory.AdminEmail, HrmsApiFactory.AdminPassword);

        response.IsSuccess.ShouldBeTrue(response.Body);
        response.Data.GetProperty("user").GetProperty("roles").EnumerateArray()
            .Select(role => role.GetString()).ShouldContain("admin");
    }

    private static string StripTraceId(string body)
        => System.Text.RegularExpressions.Regex.Replace(body, "\"traceId\":\"[^\"]*\"", "\"traceId\":\"*\"");
}
