using Domain.Enums;

namespace Application.Common.Contracts
{
    public sealed record EducationRequest(
        string School, string Major, string? Grade, int? StartYear, int? EndYear, bool IsGraduated);

    public sealed record JobExperienceRequest(
        string CompanyName, string? Department, string Position, int? StartYear, int? EndYear, string? Description);

    public sealed record CvLanguageRequest(string Name, LanguageLevel Level);

    public sealed record CvProjectRequest(string Name, string? Description);

    public sealed record SocialMediaRequest(string? Github, string? Linkedin, string? WebSite);

    public sealed record DepartmentRequest(string Name, int? NumberOfEmployees);
}
