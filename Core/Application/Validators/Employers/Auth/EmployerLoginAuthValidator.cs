using Application.Constants;
using Application.Features.EmployerAuth.Queries;
using FluentValidation;

namespace Application.Validators.Employers.Auth
{
    public class EmployerLoginAuthValidator : AbstractValidator<LoginQuery>
    {
        public EmployerLoginAuthValidator()
        {
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleFor(e => e.Password).NotEmpty().WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}