using Application.Abstractions.Storage;
using Infrastructure.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace HRMS.Application.UnitTests.Storage;

public class LocalStorageTests : IDisposable
{
    private const string Container = "cv-files";

    private readonly string _contentRoot =
        Path.Combine(Path.GetTempPath(), $"hrms-storage-{Guid.CreateVersion7():N}");

    private LocalStorage CreateSut(string? rootPath = "App_Data/uploads")
    {
        Directory.CreateDirectory(_contentRoot);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:Local:RootPath"] = rootPath })
            .Build();

        var environment = Substitute.For<IHostEnvironment>();
        environment.ContentRootPath.Returns(_contentRoot);

        return new LocalStorage(configuration, environment, NullLogger<LocalStorage>.Instance);
    }

    private static FileUploadRequest Pdf(string name = "cv.pdf")
        => new(name, "application/pdf", 4, new MemoryStream("%PDF"u8.ToArray()));

    /// <summary>
    /// Regression test for a bug that made every locally stored file unreadable.
    /// </summary>
    /// <remarks>
    /// <c>UploadAsync</c> returns a <c>StoragePath</c> that already carries the container prefix
    /// (<c>cv-files/abc.pdf</c>), and the download path passed both the container and that path back
    /// in — so the file was looked for at <c>cv-files/cv-files/abc.pdf</c> and never found. Uploads
    /// succeeded and downloads returned 404, which is the worst shape for a bug: it looks like a
    /// missing file rather than a broken path.
    /// </remarks>
    [Fact]
    public async Task OpenReadAsync_Should_FindTheFile_When_GivenTheStoragePathReturnedByUpload()
    {
        var sut = CreateSut();

        var stored = await sut.UploadAsync(Container, [Pdf()]);
        var storagePath = stored[0].StoragePath;

        storagePath.ShouldStartWith($"{Container}/", Case.Insensitive);

        await using var content = await sut.OpenReadAsync(Container, storagePath);

        content.ShouldNotBeNull();
        new StreamReader(content!).ReadToEnd().ShouldBe("%PDF");
    }

    [Fact]
    public async Task OpenReadAsync_Should_AlsoAcceptABareFileName()
    {
        var sut = CreateSut();

        var stored = await sut.UploadAsync(Container, [Pdf()]);
        var bareName = stored[0].FileName;

        await using var content = await sut.OpenReadAsync(Container, bareName);

        content.ShouldNotBeNull();
    }

    [Fact]
    public async Task OpenReadAsync_Should_ReturnNull_When_TheFileDoesNotExist()
    {
        (await CreateSut().OpenReadAsync(Container, "nope.pdf")).ShouldBeNull();
    }

    [Fact]
    public async Task UploadAsync_Should_NotOverwrite_When_TwoFilesShareAName()
    {
        var sut = CreateSut();

        await sut.UploadAsync(Container, [Pdf()]);
        var second = await sut.UploadAsync(Container, [Pdf()]);

        // The original AzureStorage passed file.Name — the form field name, identical for every file
        // in a request — so a batch upload collapsed onto a single blob.
        second[0].FileName.ShouldNotBe("cv.pdf");
        (await sut.GetFilesAsync(Container)).Count.ShouldBe(2);
    }

    /// <summary>
    /// Path traversal guard.
    /// </summary>
    /// <remarks>
    /// File names reaching storage come from database rows rather than straight off the wire, but
    /// "the caller is trusted" is exactly the assumption that turns a stored value into an
    /// arbitrary-file read.
    /// </remarks>
    [Fact]
    public async Task OpenReadAsync_Should_Refuse_When_ThePathEscapesTheStorageRoot()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => sut.OpenReadAsync(Container, "../../appsettings.json"));
    }

    /// <summary>
    /// The guard that makes the original wwwroot mistake impossible to reintroduce.
    /// </summary>
    /// <remarks>
    /// The shipped default was <c>wwwroot/uploads</c> while Program.cs calls <c>UseStaticFiles()</c>;
    /// in a published app the content root and base directory are the same folder, so every uploaded
    /// CV would have been anonymously downloadable by URL. Failing at startup beats a comment.
    /// </remarks>
    [Fact]
    public void Constructor_Should_Throw_When_TheRootWouldBeServedStatically()
    {
        var exception = Should.Throw<InvalidOperationException>(() => CreateSut("wwwroot/uploads"));

        exception.Message.ShouldContain("wwwroot");
    }

    [Fact]
    public void Constructor_Should_Accept_ARootOutsideTheWebRoot()
    {
        Should.NotThrow(() => CreateSut("App_Data/uploads"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentRoot))
        {
            Directory.Delete(_contentRoot, recursive: true);
        }

        GC.SuppressFinalize(this);
    }
}
