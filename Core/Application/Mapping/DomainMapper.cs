using Application.Common.Contracts;
using Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Application.Mapping
{
    [Mapper]
    public static partial class DomainMapper
    {
        public static partial ContactResponse ToResponse(Contact contact);

        public static partial JobPositionResponse ToResponse(JobPosition jobPosition);

        public static partial SystemStaffResponse ToResponse(SystemStaff systemStaff);

        public static partial JobSeekerResponse ToResponse(JobSeeker jobSeeker);

        public static partial EmployerResponse ToResponse(Employer employer);

        public static partial DepartmentResponse ToResponse(Department department);

        public static partial EducationResponse ToResponse(Education education);

        public static partial JobExperienceResponse ToResponse(JobExperience jobExperience);

        public static partial CvLanguageResponse ToResponse(CvLanguage cvLanguage);

        public static partial CvProjectResponse ToResponse(CvProject cvProject);

        public static partial CvFileResponse ToResponse(CvFile cvFile);

        public static SocialMediaResponse? ToResponse(Domain.ValueObjects.SocialMedia? socialMedia)
            => socialMedia is null ||
               (socialMedia.Github is null && socialMedia.Linkedin is null && socialMedia.WebSite is null)
                ? null
                : new SocialMediaResponse(socialMedia.Github, socialMedia.Linkedin, socialMedia.WebSite);

        public static UserSummaryResponse ToSummaryResponse(User user)
            => new(user.Id, user.Email, user.UserType, user.IsActive, user.CreatedAt);

        public static EmployerSummaryResponse ToSummaryResponse(Employer employer)
            => new(
                employer.Id,
                employer.CompanyName,
                employer.WebSite,
                employer.NumberOfEmployees,
                employer.Description,
                employer.Sectors);

        public static EmployerDetailResponse ToDetailResponse(Employer employer)
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
                employer.Departments.Select(ToResponse).ToList());

        public static JobAdvertisementResponse ToResponse(JobAdvertisement advertisement)
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

        public static JobApplicationResponse ToResponse(JobApplication application)
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

        public static CvResponse ToResponse(Cv cv)
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
                ToResponse(cv.SocialMedia),
                cv.Educations.Select(ToResponse).ToList(),
                cv.JobExperiences.Select(ToResponse).ToList(),
                cv.Languages.Select(ToResponse).ToList(),
                cv.Projects.Select(ToResponse).ToList(),
                cv.Files.Select(ToResponse).ToList(),
                cv.CreatedAt);
    }
}
