using Domain.Entities;
using Domain.Enums;

namespace HRMS.Persistence.IntegrationTests;

/// <summary>
/// Entity builders, so a test that is about one constraint does not open with twenty lines of
/// unrelated required fields.
/// </summary>
/// <remarks>
/// Every factory produces a valid, saveable graph; a test that wants an invalid one breaks exactly
/// the field it is testing, which keeps the intent of each case visible.
/// </remarks>
internal static class Given
{
    public static JobSeeker JobSeeker(string email = "seeker@test.local", string? nationalId = null)
        => new()
        {
            Email = email,
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Seeker",
            NationalId = nationalId
        };

    public static Employer Employer(string email = "employer@test.local", string company = "Acme")
        => new()
        {
            Email = email,
            PasswordHash = "hash",
            CompanyName = company
        };

    public static SystemStaff SystemStaff(string email = "staff@test.local")
        => new()
        {
            Email = email,
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Staff"
        };

    public static JobPosition JobPosition(string name = "Backend Developer")
        => new() { Name = name };

    /// <summary>A CV with one row in each child collection, so cascades have something to remove.</summary>
    public static Cv Cv(Guid jobSeekerId)
        => new()
        {
            JobSeekerId = jobSeekerId,
            Information = "Bir özgeçmiş.",
            Skills = ["csharp", "postgresql"],
            Educations = { new Education { School = "Üniversite", Major = "Bilgisayar", StartYear = 2015, EndYear = 2019 } },
            JobExperiences = { new JobExperience { CompanyName = "Eski İşveren", Position = "Geliştirici" } },
            Languages = { new CvLanguage { Name = "İngilizce", Level = LanguageLevel.Advanced } },
            Projects = { new CvProject { Name = "Bir proje" } },
            Files = { new CvFile { FileName = "cv.pdf", StoragePath = "cv-files/cv.pdf", StorageProvider = StorageProvider.Local } }
        };

    public static JobAdvertisement JobAdvertisement(Guid employerId, Guid jobPositionId, string title = "Backend Engineer")
        => new()
        {
            EmployerId = employerId,
            JobPositionId = jobPositionId,
            Title = title,
            Description = "Bu ilan açıklaması yeterince uzundur.",
            Skills = ["csharp"],
            OpenPositions = 1,
            JobType = JobType.FullTime,
            Deadline = new DateOnly(2027, 1, 1),
            IsActive = true
        };

    public static JobApplication JobApplication(Guid advertisementId, Guid jobSeekerId)
        => new()
        {
            JobAdvertisementId = advertisementId,
            JobSeekerId = jobSeekerId,
            Status = JobApplicationStatus.Submitted
        };

    public static Contact Contact(string email = "contact@test.local")
        => new()
        {
            FirstName = "Ad",
            LastName = "Soyad",
            Email = email,
            Subject = "Konu",
            Message = "Yeterince uzun bir mesaj gövdesi."
        };
}
