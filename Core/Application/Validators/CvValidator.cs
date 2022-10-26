using Domain.Entities;
using FluentValidation;

namespace Application.Validators
{
    public class CvValidator : AbstractValidator<Cv>
    {
        public CvValidator()
        {
            RuleFor(cv => cv.Information).NotEmpty().MinimumLength(25).MaximumLength(250);
            When(cv => cv.JobExperience, () =>
            {
                RuleForEach(cv => cv.JobExperiences.OfType<JobExperience>()).NotNull().OverridePropertyName("JobExperience").SetValidator(new JobExperienceValidator());
            });
            When(cv => cv.ProgrammingLanguage, () =>
            {
                RuleForEach(cv => cv.ProgrammingLanguages.OfType<ProgrammingLanguage>()).NotNull().OverridePropertyName("ProgrammingLanguage").SetValidator(new ProgrammingLanguageValidator());
            });
            When(cv => cv.Language, () =>
            {
                RuleForEach(cv => cv.Languages.OfType<Language>()).NotNull().OverridePropertyName("Language").SetValidator(new LanguageValidator());
            });
            When(cv => cv.LibraryAndFramework, () =>
            {
                RuleForEach(cv => cv.LibraryAndFrameworks.OfType<LibraryAndFramework>()).NotNull().OverridePropertyName("LibraryAndFramework").SetValidator(new LibraryAndFrameworkValidator());
            });
            When(cv => cv.Project, () =>
            {
                RuleForEach(cv => cv.Projects.OfType<Project>()).NotNull().OverridePropertyName("Project").SetValidator(new ProjectValidator());
            });
            When(cv => cv.SocialMedia, () =>
            {
                RuleFor(cv => cv.SocialMedias).NotNull().OverridePropertyName("SocialMedia").SetValidator(new SocialMediaValidator());
            });
            When(cv => cv.Hobby, () =>
            {
                RuleFor(cv => cv.Hobbies).NotNull().OverridePropertyName("Hobby").SetValidator(new HobbyValidator());
            });
            RuleForEach(cv => cv.Educations.OfType<Education>()).NotNull().OverridePropertyName("Education").SetValidator(new EducationValidator());
        }
    }
}