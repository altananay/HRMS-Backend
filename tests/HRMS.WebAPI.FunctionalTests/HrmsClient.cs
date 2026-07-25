using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// A thin, typed wrapper over <see cref="HttpClient"/> so scenarios read as prose.
/// </summary>
/// <remarks>
/// Deliberately returns status codes rather than throwing, because most of what these tests assert
/// <i>is</i> the status code — 401 vs 403 vs 409 is the behaviour under test, not an error.
/// </remarks>
public sealed class HrmsClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public sealed record Response(HttpStatusCode Status, string Body)
    {
        public bool IsSuccess => (int)Status is >= 200 and < 300;

        /// <summary>Reads a value out of the <c>data</c> envelope every success response uses.</summary>
        public JsonElement Data => JsonDocument.Parse(Body).RootElement.GetProperty("data");

        public string DataString(string property) => Data.GetProperty(property).GetString()!;
    }

    public void Authenticate(string? accessToken)
        => http.DefaultRequestHeaders.Authorization =
            accessToken is null ? null : new AuthenticationHeaderValue("Bearer", accessToken);

    public async Task<Response> GetAsync(string path)
        => await ReadAsync(await http.GetAsync(path, TestContext.Current.CancellationToken));

    public async Task<Response> PostAsync(string path, object? body = null)
        => await ReadAsync(await http.PostAsJsonAsync(path, body ?? new { }, Json, TestContext.Current.CancellationToken));

    public async Task<Response> PutAsync(string path, object body)
        => await ReadAsync(await http.PutAsJsonAsync(path, body, Json, TestContext.Current.CancellationToken));

    public async Task<Response> DeleteAsync(string path)
        => await ReadAsync(await http.DeleteAsync(path, TestContext.Current.CancellationToken));

    /// <summary>Multipart upload, since CV attachments cannot go through JSON.</summary>
    public async Task<Response> UploadAsync(string path, string fileName, string contentType, byte[] content)
    {
        using var form = new MultipartFormDataContent();
        var part = new ByteArrayContent(content);
        part.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // Field name must be "files" to bind to the IFormFileCollection parameter.
        form.Add(part, "files", fileName);

        return await ReadAsync(await http.PostAsync(path, form, TestContext.Current.CancellationToken));
    }

    private static async Task<Response> ReadAsync(HttpResponseMessage message)
        => new(message.StatusCode, await message.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

    // ---------------------------------------------------------------------------------------------
    // Scenario helpers
    // ---------------------------------------------------------------------------------------------

    public Task<Response> RegisterJobSeekerAsync(string email, string password = "Passw0rd!23")
        => PostAsync("/api/auth/register/jobseeker", new
        {
            email,
            password,
            firstName = "Test",
            lastName = "Seeker"
        });

    public Task<Response> RegisterEmployerAsync(string email, string company = "Acme", string password = "Passw0rd!23")
        => PostAsync("/api/auth/register/employer", new { email, password, companyName = company });

    public Task<Response> LoginAsync(string email, string password)
        => PostAsync("/api/auth/login", new { email, password });

    /// <summary>Logs in and attaches the resulting access token to subsequent requests.</summary>
    public async Task<(string AccessToken, string RefreshToken)> LoginAsAsync(string email, string password)
    {
        var response = await LoginAsync(email, password);
        response.IsSuccess.ShouldBeTrue($"login for {email} failed: {response.Body}");

        var tokens = (response.DataString("accessToken"), response.DataString("refreshToken"));
        Authenticate(tokens.Item1);

        return tokens;
    }

    public Task<Response> PostAdvertisementAsync(string title, string position = "Backend Developer")
        => PostAsync("/api/JobAdvertisements/add", new
        {
            title,
            jobPositionName = position,
            description = "Bu ilan açıklaması doğrulamadan geçecek kadar uzun olmalıdır.",
            openPositions = 2,
            jobType = "FullTime",
            deadline = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            skills = new[] { "csharp" },
            city = "İstanbul"
        });
}
