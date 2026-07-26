using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Storage;
using Application.Common.Exceptions;
using Application.Features.Cvs.Commands;
using Application.Rules;
using Application.Services;
using Application.Utilities.Constants;
using Domain.Entities;
using Domain.Enums;

namespace HRMS.Application.UnitTests.Services;

/// <summary>
/// Upload validation and download authorization for CV attachments.
/// </summary>
/// <remarks>
/// These are exactly the rules that are cheap to get wrong and expensive to discover in production:
/// a CV is personal data, and the employer case is a data question ("did this seeker apply to me?")
/// rather than a role check, so <c>[Authorize(Roles = "employer")]</c> alone would hand every CV in
/// the system to every employer.
///
/// Substituting the repositories is what the per-aggregate interface design was chosen for — none of
/// this needs a database.
/// </remarks>
public class CvFileManagerTests
{
    private static readonly Guid OwnerId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid OtherSeekerId = Guid.Parse("00000000-0000-0000-0000-0000000000a2");
    private static readonly Guid EmployerId = Guid.Parse("00000000-0000-0000-0000-0000000000b1");
    private static readonly Guid CvId = Guid.Parse("00000000-0000-0000-0000-0000000000c1");
    private static readonly Guid FileId = Guid.Parse("00000000-0000-0000-0000-0000000000d1");

    private readonly ICvRepository _cvs = Substitute.For<ICvRepository>();
    private readonly ICvFileRepository _cvFiles = Substitute.For<ICvFileRepository>();
    private readonly IJobApplicationRepository _applications = Substitute.For<IJobApplicationRepository>();
    private readonly IStorageService _storage = Substitute.For<IStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    private CvFileManager CreateSut()
    {
        var rules = new BusinessRules(
            Substitute.For<IJobSeekerRepository>(),
            Substitute.For<IEmployerRepository>(),
            _cvs,
            Substitute.For<IJobAdvertisementRepository>(),
            _applications,
            Substitute.For<IJobPositionRepository>(),
            Substitute.For<IContactRepository>(),
            Substitute.For<IUserRepository>());

        // The real policy, not a substitute: it is the thing under test in the authorization cases
        // below, and it is shared with CvManager and JobSeekerManager, so a fake here would let the
        // three drift apart silently.
        var access = new CandidateAccessPolicy(_applications, _currentUser);

        return new CvFileManager(_cvs, _cvFiles, _storage, _unitOfWork, rules, access);
    }

    private Cv GivenCvExists(int existingFileCount = 0)
    {
        var cv = new Cv { Id = CvId, JobSeekerId = OwnerId };

        for (var i = 0; i < existingFileCount; i++)
        {
            cv.Files.Add(new CvFile { CvId = CvId, FileName = $"existing-{i}.pdf", StoragePath = $"cv-files/{i}.pdf" });
        }

        _cvs.GetByJobSeekerIdAsync(OwnerId, Arg.Any<CancellationToken>()).Returns(cv);
        return cv;
    }

    private static UploadCvFileCommand Upload(string fileName, string? contentType, long length)
        => new()
        {
            JobSeekerId = OwnerId,
            Files = [new FileUploadRequest(fileName, contentType, length, new MemoryStream([1, 2, 3]))]
        };

    // ---------------------------------------------------------------------------------------------
    // Upload validation
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task UploadAsync_Should_StoreFile_When_ItIsAValidPdf()
    {
        GivenCvExists();
        _storage.Provider.Returns(StorageProvider.Local);
        _storage.UploadAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<FileUploadRequest>>(), Arg.Any<CancellationToken>())
            .Returns([new StoredFile("cv.pdf", "cv-files/abc.pdf", "application/pdf", 1024)]);

        var result = await CreateSut().UploadAsync(Upload("cv.pdf", "application/pdf", 1024));

        result.IsSuccess.ShouldBeTrue();
        result.Data.Count.ShouldBe(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadAsync_Should_Reject_When_FileExceedsSizeLimit()
    {
        GivenCvExists();

        await Should.ThrowAsync<ConflictException>(
            () => CreateSut().UploadAsync(Upload("cv.pdf", "application/pdf", 6 * 1024 * 1024)));

        // Nothing may reach storage once validation fails.
        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default);
    }

    [Fact]
    public async Task UploadAsync_Should_Reject_When_ContentTypeIsNotADocument()
    {
        GivenCvExists();

        await Should.ThrowAsync<ConflictException>(
            () => CreateSut().UploadAsync(Upload("cv.png", "image/png", 1024)));
    }

    /// <summary>
    /// The check that stops <c>payload.exe</c> arriving labelled <c>application/pdf</c>.
    /// </summary>
    /// <remarks>
    /// A content type is caller-supplied and trivially spoofed, and an extension says nothing about
    /// the bytes — requiring the two to agree is the cheap defence that catches the obvious attempt.
    /// </remarks>
    [Fact]
    public async Task UploadAsync_Should_Reject_When_ExtensionContradictsContentType()
    {
        GivenCvExists();

        await Should.ThrowAsync<ConflictException>(
            () => CreateSut().UploadAsync(Upload("payload.exe", "application/pdf", 1024)));

        await _storage.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default);
    }

    [Fact]
    public async Task UploadAsync_Should_Reject_When_FileIsEmpty()
    {
        GivenCvExists();

        await Should.ThrowAsync<ConflictException>(
            () => CreateSut().UploadAsync(Upload("cv.pdf", "application/pdf", 0)));
    }

    [Fact]
    public async Task UploadAsync_Should_Reject_When_CvAlreadyHoldsTheMaximumNumberOfFiles()
    {
        GivenCvExists(existingFileCount: 5);

        await Should.ThrowAsync<ConflictException>(
            () => CreateSut().UploadAsync(Upload("cv.pdf", "application/pdf", 1024)));
    }

    [Fact]
    public async Task UploadAsync_Should_Throw_When_TheSeekerHasNoCv()
    {
        _cvs.GetByJobSeekerIdAsync(OwnerId, Arg.Any<CancellationToken>()).Returns((Cv?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => CreateSut().UploadAsync(Upload("cv.pdf", "application/pdf", 1024)));
    }

    // ---------------------------------------------------------------------------------------------
    // Download authorization
    // ---------------------------------------------------------------------------------------------

    private void GivenStoredFile()
    {
        var cv = new Cv { Id = CvId, JobSeekerId = OwnerId };
        var file = new CvFile
        {
            Id = FileId,
            CvId = CvId,
            Cv = cv,
            FileName = "cv.pdf",
            StoragePath = "cv-files/abc.pdf",
            ContentType = "application/pdf"
        };

        _cvFiles.GetByIdWithCvAsync(FileId, Arg.Any<CancellationToken>()).Returns(file);
        _storage.OpenReadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream([1, 2, 3]));
    }

    [Fact]
    public async Task DownloadAsync_Should_Succeed_When_CallerOwnsTheCv()
    {
        GivenStoredFile();

        var download = await CreateSut().DownloadAsync(FileId, OwnerId);

        download.FileName.ShouldBe("cv.pdf");
        download.ContentType.ShouldBe("application/pdf");
    }

    [Fact]
    public async Task DownloadAsync_Should_Forbid_When_CallerIsAnUnrelatedJobSeeker()
    {
        GivenStoredFile();

        await Should.ThrowAsync<ForbiddenException>(() => CreateSut().DownloadAsync(FileId, OtherSeekerId));
    }

    /// <summary>
    /// Holding the employer role is not enough — the seeker must actually have applied to them.
    /// </summary>
    [Fact]
    public async Task DownloadAsync_Should_Forbid_When_EmployerHasNoApplicationFromTheSeeker()
    {
        GivenStoredFile();
        _currentUser.IsInRole(Roles.Employer).Returns(true);
        _applications.ExistsForEmployerAndSeekerAsync(EmployerId, OwnerId, Arg.Any<CancellationToken>()).Returns(false);

        await Should.ThrowAsync<ForbiddenException>(() => CreateSut().DownloadAsync(FileId, EmployerId));
    }

    [Fact]
    public async Task DownloadAsync_Should_Succeed_When_EmployerHasAnApplicationFromTheSeeker()
    {
        GivenStoredFile();
        _currentUser.IsInRole(Roles.Employer).Returns(true);
        _applications.ExistsForEmployerAndSeekerAsync(EmployerId, OwnerId, Arg.Any<CancellationToken>()).Returns(true);

        var download = await CreateSut().DownloadAsync(FileId, EmployerId);

        download.FileName.ShouldBe("cv.pdf");
    }

    [Fact]
    public async Task DownloadAsync_Should_Succeed_When_CallerIsAdmin()
    {
        GivenStoredFile();
        _currentUser.IsInRole(Roles.Admin).Returns(true);

        var download = await CreateSut().DownloadAsync(FileId, OtherSeekerId);

        download.FileName.ShouldBe("cv.pdf");
    }

    [Fact]
    public async Task DownloadAsync_Should_ReturnNotFound_When_TheBlobIsMissingFromStorage()
    {
        GivenStoredFile();
        _storage.OpenReadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Stream?)null);

        // A database row without its object is a 404, not a 500.
        await Should.ThrowAsync<NotFoundException>(() => CreateSut().DownloadAsync(FileId, OwnerId));
    }

    // ---------------------------------------------------------------------------------------------
    // Delete
    // ---------------------------------------------------------------------------------------------

    /// <remarks>
    /// Deletion is stricter than reading: an employer who may download a CV must not be able to
    /// destroy it.
    /// </remarks>
    [Fact]
    public async Task DeleteAsync_Should_Forbid_When_CallerIsAnEmployerWithAnApplication()
    {
        GivenStoredFile();
        _currentUser.IsInRole(Roles.Employer).Returns(true);
        _applications.ExistsForEmployerAndSeekerAsync(EmployerId, OwnerId, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<ForbiddenException>(() => CreateSut().DeleteAsync(FileId, EmployerId));
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveTheBlobAndTheRow_When_CallerOwnsTheCv()
    {
        GivenStoredFile();

        var result = await CreateSut().DeleteAsync(FileId, OwnerId);

        result.IsSuccess.ShouldBeTrue();
        await _storage.Received(1).DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _cvFiles.Received(1).Remove(Arg.Any<CvFile>());
    }
}
