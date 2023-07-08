using Application.Utilities.Constants;
using Domain.Entities;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.JobSeekers
{
    public class JobSeekerValidator : AbstractValidator<JobSeeker>
    {
        public JobSeekerValidator()
        {
            RuleFor(j => j.FirstName).NotEmpty().WithMessage(ValidationMessages.FirstNameCantBeEmpty);
            RuleFor(j => j.LastName).NotEmpty().WithMessage(ValidationMessages.LastNameCantBeEmpty);
            RuleFor(j => j.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleFor(j => j.PasswordHash).NotEmpty().WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}