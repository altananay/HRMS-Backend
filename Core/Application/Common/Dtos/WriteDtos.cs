using Domain.Enums;

namespace Application.Common.Dtos
{
    /// <summary>Child items supplied when creating or replacing a CV.</summary>
    /// <remarks>
    /// No <c>Id</c> on these. CvManager used to generate a fresh ObjectId for every project and
    /// experience on <i>every</i> update, so child identity was never stable anyway; the CV
    /// aggregate is now replaced wholesale on update, which is honest about that and keeps the
    /// handler simple.
    /// </remarks>
    public sealed record EducationInput(
        string School, string Major, string? Grade, int? StartYear, int? EndYear, bool IsGraduated);

    public sealed record JobExperienceInput(
        string CompanyName, string? Department, string Position, int? StartYear, int? EndYear, string? Description);

    public sealed record CvLanguageInput(string Name, LanguageLevel Level);

    public sealed record CvProjectInput(string Name, string? Description);

    public sealed record SocialMediaInput(string? Github, string? Linkedin, string? WebSite);

    public sealed record DepartmentInput(string Name, int? NumberOfEmployees);
}
