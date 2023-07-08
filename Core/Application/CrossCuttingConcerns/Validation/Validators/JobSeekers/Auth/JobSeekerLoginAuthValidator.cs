using Application.Features.Auth.Queries;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.JobSeekers.Auth
{
    public class JobSeekerLoginAuthValidator : AbstractValidator<JobSeekerLoginQuery>
    {
        public JobSeekerLoginAuthValidator()
        {
            RuleFor(a => a.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleFor(a => a.Password).NotEmpty().WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}