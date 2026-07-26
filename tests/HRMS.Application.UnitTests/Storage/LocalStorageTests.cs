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

        second[0].FileName.ShouldNotBe("cv.pdf");
        (await sut.GetFilesAsync(Container)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task OpenReadAsync_Should_Refuse_When_ThePathEscapesTheStorageRoot()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => sut.OpenReadAsync(Container, "../../appsettings.json"));
    }

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
