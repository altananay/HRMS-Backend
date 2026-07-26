using Domain.Enums;

namespace Application.Common.Dtos
{
    /// <summary>
    /// Read models returned to clients.
    /// </summary>
    /// <remarks>
    /// Every list and detail endpoint projects into one of these instead of serializing an entity.
    /// That is what makes the old leak impossible rather than merely fixed: <c>GET
    /// /api/JobSeekers/getall</c> and <c>GET /api/Employers/getall</c> returned raw entities to
    /// anonymous callers, so <c>PasswordHash</c> and <c>PasswordSalt</c> went out on the wire. A DTO
    /// with no such property cannot expose one no matter who calls it.
    ///
    /// National IDs are likewise absent from every DTO here — TCKN is PII and has no business in a
    /// list response.
    /// </remarks>
    /// <summary>What a successful create returns: the identity of the row that now exists.</summary>
    /// <remarks>
    /// Creates used to answer <c>{ "isSuccess": true, "message": "..." }</c> and nothing else, so a
    /// client that had just published an advertisement had no way to address it — the only route to
    /// its id was to list everything and search by title. The functional tests did exactly that,
    /// which is how obvious the gap was.
    ///
    /// An object rather than a bare <c>Guid</c> in <c>data</c>: every other payload under that key
    /// is an object with an <c>id</c>, so clients keep one shape to parse, and a create that later
    /// needs to return a second field extends without breaking the contract.
    /// </remarks>
    public sealed record CreatedDto(Guid Id);

    public sealed record JobSeekerDto(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        DateOnly? DateOfBirth,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record EmployerDto(
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

    public sealed record EmployerDetailDto(
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
        IReadOnlyList<DepartmentDto> Departments);

    public sealed record DepartmentDto(Guid Id, string Name, int? NumberOfEmployees);

    public sealed record SystemStaffDto(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        bool IsActive,
        DateTime CreatedAt);

    public sealed record UserSummaryDto(
        Guid Id,
        string Email,
        UserType UserType,
        bool IsActive,
        DateTime CreatedAt);

    /// <summary>The caller's own identity, returned by login/refresh and by <c>GET /api/auth/me</c>.</summary>
    public sealed record AuthenticatedUserDto(
        Guid Id,
        string Email,
        string DisplayName,
        UserType UserType,
        IReadOnlyList<string> Roles);

    /// <summary>
    /// An advertisement as the client sees it.
    /// </summary>
    /// <remarks>
    /// <c>CompanyName</c>, <c>CompanyPhone</c>, <c>WebSite</c>, <c>Email</c> and
    /// <c>JobPositionName</c> are projected from the Employer and JobPosition navigations. They used
    /// to be denormalized columns stamped onto every advertisement row at insert and never
    /// refreshed afterwards. Projecting them keeps the JSON contract byte-identical for the frontend
    /// while the database holds one copy.
    /// </remarks>
    public sealed record JobAdvertisementDto(
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

    public sealed record JobPositionDto(Guid Id, string Name);

    public sealed record JobApplicationDto(
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

    public sealed record ContactDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Subject,
        string Message,
        bool IsHandled,
        DateTime CreatedAt);

    public sealed record CvDto(
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
        SocialMediaDto? SocialMedia,
        IReadOnlyList<EducationDto> Educations,
        IReadOnlyList<JobExperienceDto> JobExperiences,
        IReadOnlyList<CvLanguageDto> Languages,
        IReadOnlyList<CvProjectDto> Projects,
        IReadOnlyList<CvFileDto> Files,
        DateTime CreatedAt);

    public sealed record SocialMediaDto(string? Github, string? Linkedin, string? WebSite);

    public sealed record EducationDto(
        Guid Id, string School, string Major, string? Grade, int? StartYear, int? EndYear, bool IsGraduated);

    public sealed record JobExperienceDto(
        Guid Id, string CompanyName, string? Department, string Position, int? StartYear, int? EndYear, string? Description);

    public sealed record CvLanguageDto(Guid Id, string Name, LanguageLevel Level);

    public sealed record CvProjectDto(Guid Id, string Name, string? Description);

    public sealed record CvFileDto(
        Guid Id, string FileName, StorageProvider StorageProvider, string? ContentType, long SizeBytes, DateTime CreatedAt);
}
