using Application.Constants;
using Application.Features.Auth.Queries;
using FluentValidation;

namespace Application.Validators.JobSeekers.Auth
{
    public class JobSeekerLoginAuthValidator : AbstractValidator<AuthQuery>
    {
        public JobSeekerLoginAuthValidator()
        {
            RuleFor(a => a.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleFor(a => a.Password).NotEmpty().WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}