using Application.Features.EmployerAuth.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Employers.Auth
{
    public class EmployerValidator : AbstractValidator<EmployerRegisterCommand>
    {
        public EmployerValidator()
        {
            RuleFor(e => e.CompanyName).NotEmpty().MinimumLength(5).WithMessage(ValidationMessages.CompanyNameCantEmpty);
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailCantEmpty);
            RuleFor(e => e.WebSite).NotEmpty().WithMessage(ValidationMessages.WebSiteCantEmpty);
            RuleFor(e => e.CompanyPhone).NotEmpty().WithMessage(ValidationMessages.PhoneNumberCantEmpty);
            RuleFor(e => e.Password).NotEmpty().MinimumLength(5).MaximumLength(10).WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}