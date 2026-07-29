using Domain.Enums;

namespace Application.Common.Contracts
{
    public sealed record CreatedResponse(Guid Id);

    public sealed record JobSeekerResponse(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        DateOnly? DateOfBirth,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record EmployerResponse(
        Guid Id,
        string Email,
        string CompanyName,
        string? CompanyPhone,
        string? WebSite,
        int? NumberOfEmployees,
        string? Description,
        string[] Sectors,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record EmployerDetailResponse(
        Guid Id,
        string Email,
        string CompanyName,
        string? CompanyPhone,
        string? WebSite,
        int? NumberOfEmployees,
        string? Description,
        string[] Sectors,
        bool IsActive,
        DateTime CreatedAt,
        IReadOnlyList<DepartmentResponse> Departments);

    /// <summary>
    /// An employer as the public company directory sees them.
    /// </summary>
    /// <remarks>
    /// Deliberately narrower than <see cref="EmployerResponse"/>: no <c>Email</c> and no
    /// <c>IsActive</c>. An anonymous, paged list of employer email addresses is a scraping target,
    /// and the account's status is nobody's business but the admin's. The address is still shown on
    /// the company's own detail page, which is what <see cref="EmployerDetailResponse"/> serves.
    /// </remarks>
    public sealed record EmployerSummaryResponse(
        Guid Id,
        string CompanyName,
        string? WebSite,
        int? NumberOfEmployees,
        string? Description,
        string[] Sectors);

    public sealed record DepartmentResponse(Guid Id, string Name, int? NumberOfEmployees);

    public sealed record SystemStaffResponse(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record UserSummaryResponse(
        Guid Id,
        string Email,
        UserType UserType,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record AuthenticatedUserResponse(
        Guid Id,
        string Email,
        string DisplayName,
        UserType UserType,
        IReadOnlyList<string> Roles);

    public sealed record JobAdvertisementResponse(
        Guid Id,
        Guid EmployerId,
        string CompanyName,
        string? CompanyPhone,
        string? WebSite,
        string Email,
        Guid JobPositionId,
        string JobPositionName,
        string Title,
        string Description,
        string? Experience,
        string[] Skills,
        string? City,
        decimal? MinSalary,
        decimal? MaxSalary,
        string? Currency,
        int OpenPositions,
        JobType JobType,
        DateOnly Deadline,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record JobPositionResponse(Guid Id, string Name);

    public sealed record JobApplicationResponse(
        Guid Id,
        Guid JobAdvertisementId,
        string JobAdvertisementTitle,
        Guid EmployerId,
        Guid JobSeekerId,
        string JobSeekerFullName,
        string? JobSeekerNote,
        string? EmployerNote,
        JobApplicationStatus Status,
        DateTime? StatusChangedAt,
        DateTime CreatedAt);

    public sealed record ContactResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Subject,
        string Message,
        bool IsHandled,
        DateTime CreatedAt);

    public sealed record CvResponse(
        Guid Id,
        Guid JobSeekerId,
        string FirstName,
        string LastName,
        string Email,
        DateOnly? DateOfBirth,
        string? Information,
        string? ImageUrl,
        string? Hobbies,
        string[] Skills,
        SocialMediaResponse? SocialMedia,
        IReadOnlyList<EducationResponse> Educations,
        IReadOnlyList<JobExperienceResponse> JobExperiences,
        IReadOnlyList<CvLanguageResponse> Languages,
        IReadOnlyList<CvProjectResponse> Projects,
        IReadOnlyList<CvFileResponse> Files,
        DateTime CreatedAt);

    public sealed record SocialMediaResponse(string? Github, string? Linkedin, string? WebSite);

    public sealed record EducationResponse(
        Guid Id, string School, string Major, string? Grade, int? StartYear, int? EndYear, bool IsGraduated);

    public sealed record JobExperienceResponse(
        Guid Id, string CompanyName, string? Department, string Position, int? StartYear, int? EndYear, string? Description);

    public sealed record CvLanguageResponse(Guid Id, string Name, LanguageLevel Level);

    public sealed record CvProjectResponse(Guid Id, string Name, string? Description);

    public sealed record CvFileResponse(
        Guid Id, string FileName, StorageProvider StorageProvider, string? ContentType, long SizeBytes, DateTime CreatedAt);
}
