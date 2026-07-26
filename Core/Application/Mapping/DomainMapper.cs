using Application.Common.Dtos;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mapping
{
    /// <summary>
    /// Entity to DTO mapping, generated at compile time by Riok.Mapperly.
    /// </summary>
    /// <remarks>
    /// Replaces AutoMapper, for two reasons.
    ///
    /// <b>Licensing:</b> AutoMapper 15+ is commercial. The project was pinned at 12.0.1 — the last
    /// free release, which also carries a published high-severity advisory.
    ///
    /// <b>Correctness:</b> Mapperly is a source generator, so an unmapped member is a build error
    /// (RMG012/RMG020) rather than a silently-null property discovered at runtime. The old
    /// AutoMapperConfig never called <c>AssertConfigurationIsValid()</c>, and had maps for
    /// <c>CreateProjectDto</c> but none for <c>UpdateProjectDto</c> even though CvManager mapped
    /// both — exactly the class of mistake that only surfaces in production.
    ///
    /// Nothing here maps a DTO back onto an entity. Mapping untrusted input straight onto a
    /// persisted object is what let a client set <c>SystemStaff.Claims</c> to "admin"; writes go
    /// through explicit assignment in the managers instead.
    /// </remarks>
    [Mapper]
    public static partial class DomainMapper
    {
        public static partial ContactDto ToDto(Contact contact);

        public static partial JobPositionDto ToDto(JobPosition jobPosition);

        public static partial SystemStaffDto ToDto(SystemStaff systemStaff);

        public static partial JobSeekerDto ToDto(JobSeeker jobSeeker);

        public static partial EmployerDto ToDto(Employer employer);

        public static partial DepartmentDto ToDto(Department department);

        public static partial EducationDto ToDto(Education education);

        public static partial JobExperienceDto ToDto(JobExperience jobExperience);

        public static partial CvLanguageDto ToDto(CvLanguage cvLanguage);

        public static partial CvProjectDto ToDto(CvProject cvProject);

        public static partial CvFileDto ToDto(CvFile cvFile);

        /// <summary>
        /// Hand-written: the target is a positional record, and an instance whose three links are all
        /// empty is reported as absent.
        /// </summary>
        /// <remarks>
        /// The emptiness check is what keeps the JSON contract stable. <c>Cv.SocialMedia</c> is a
        /// required owned navigation, so it is never null in memory; without collapsing the empty
        /// case here, a CV with no links would start serializing
        /// <c>"socialMedia": { "github": null, "linkedin": null, "webSite": null }</c> where it
        /// previously sent <c>"socialMedia": null</c>.
        /// </remarks>
        public static SocialMediaDto? ToDto(Domain.ValueObjects.SocialMedia? socialMedia)
            => socialMedia is null ||
               (socialMedia.Github is null && socialMedia.Linkedin is null && socialMedia.WebSite is null)
                ? null
                : new SocialMediaDto(socialMedia.Github, socialMedia.Linkedin, socialMedia.WebSite);

        public static UserSummaryDto ToSummaryDto(User user)
            => new(user.Id, user.Email, user.UserType, user.IsActive, user.CreatedAt);

        public static EmployerDetailDto ToDetailDto(Employer employer)
            => new(
                employer.Id,
                employer.Email,
                employer.CompanyName,
                employer.CompanyPhone,
                employer.WebSite,
                employer.NumberOfEmployees,
                employer.Description,
                employer.Sectors,
                employer.IsActive,
                employer.CreatedAt,
                employer.Departments.Select(ToDto).ToList());

        /// <summary>
        /// Flattens the employer and position navigations into the advertisement response.
        /// </summary>
        /// <remarks>
        /// Written by hand rather than generated: these five values come from two navigation
        /// properties and used to be denormalized columns. Keeping the flattening explicit is what
        /// guarantees the JSON contract the frontend already consumes stays unchanged.
        /// </remarks>
        public static JobAdvertisementDto ToDto(JobAdvertisement advertisement)
            => new(
                advertisement.Id,
                advertisement.EmployerId,
                advertisement.Employer.CompanyName,
                advertisement.Employer.CompanyPhone,
                advertisement.Employer.WebSite,
                advertisement.Employer.Email,
                advertisement.JobPositionId,
                advertisement.JobPosition.Name,
                advertisement.Title,
                advertisement.Description,
                advertisement.Experience,
                advertisement.Skills,
                advertisement.City,
                advertisement.MinSalary,
                advertisement.MaxSalary,
                advertisement.Currency,
                advertisement.OpenPositions,
                advertisement.JobType,
                advertisement.Deadline,
                advertisement.IsActive,
                advertisement.CreatedAt);

        public static JobApplicationDto ToDto(JobApplication application)
            => new(
                application.Id,
                application.JobAdvertisementId,
                application.JobAdvertisement.Title,
                application.JobAdvertisement.EmployerId,
                application.JobSeekerId,
                $"{application.JobSeeker.FirstName} {application.JobSeeker.LastName}",
                application.JobSeekerNote,
                application.EmployerNote,
                application.Status,
                application.StatusChangedAt,
                application.CreatedAt);

        public static CvDto ToDto(Cv cv)
            => new(
                cv.Id,
                cv.JobSeekerId,
                cv.JobSeeker.FirstName,
                cv.JobSeeker.LastName,
                cv.JobSeeker.Email,
                cv.JobSeeker.DateOfBirth,
                cv.Information,
                cv.ImageUrl,
                cv.Hobbies,
                cv.Skills,
                ToDto(cv.SocialMedia),
                cv.Educations.Select(ToDto).ToList(),
                cv.JobExperiences.Select(ToDto).ToList(),
                cv.Languages.Select(ToDto).ToList(),
                cv.Projects.Select(ToDto).ToList(),
                cv.Files.Select(ToDto).ToList(),
                cv.CreatedAt);
    }
}
