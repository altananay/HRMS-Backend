﻿using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Features.Cvs.Commands;
using Application.Utilities.Constants;
using Domain.Objects;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Cvs
{
    public class UpdateCvValidator : AbstractValidator<UpdateCvCommand>
    {
        public UpdateCvValidator()
        {
            RuleFor(cv => cv.Information).NotEmpty().MinimumLength(20).MaximumLength(250);
            RuleForEach(cv => cv.Educations.OfType<Education>()).NotNull().OverridePropertyName("Education").SetValidator(new EducationValidator());
            //RuleFor(cv => cv.Hobbies).NotEmpty().MinimumLength(10).MaximumLength(250);
            RuleFor(cv => cv.Skills).NotEmpty();
            //RuleForEach(cv => cv.Projects.OfType<Project>()).NotNull().OverridePropertyName("Project").SetValidator(new ProjectValidator());
            //RuleFor(cv => cv.SocialMedias.WebSite).NotEmpty().MinimumLength(7).MaximumLength(30);
            //RuleFor(cv => cv.SocialMedias.Github).NotEmpty().MinimumLength(5).MaximumLength(50);
            //RuleFor(cv => cv.SocialMedias.Linkedin).NotEmpty().MinimumLength(5).MaximumLength(100);
            //RuleForEach(cv => cv.JobExperiences.OfType<JobExperience>()).NotNull().OverridePropertyName("JobExperience").SetValidator(new JobExperienceValidator());
            RuleFor(cv => cv.JobSeekerId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            //RuleForEach(cv => cv.Languages.OfType<Language>()).NotNull().OverridePropertyName("Language").SetValidator(new LanguageValidator());
            RuleFor(cv => cv.Id).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
        }
    }
}
