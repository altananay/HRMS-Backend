using Application.Features.EmployerAuth.Queries;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Employers.Auth
{
    public class EmployerLoginAuthValidator : AbstractValidator<EmployerLoginQuery>
    {
        public EmployerLoginAuthValidator()
        {
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleFor(e => e.Password).NotEmpty().WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}