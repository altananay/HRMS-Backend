using Application.Features.Employers.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Employers
{
    public class UpdateEmployerValidator : AbstractValidator<UpdateEmployerCommand>
    {
        public UpdateEmployerValidator()
        {
            RuleFor(e => e.CompanyName).NotEmpty().MinimumLength(5).WithMessage(ValidationMessages.CompanyNameCantEmpty);
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailCantEmpty);
            RuleFor(e => e.WebSite).NotEmpty().WithMessage(ValidationMessages.WebSiteCantEmpty);
            RuleFor(e => e.CompanyPhone).NotEmpty().WithMessage(ValidationMessages.PhoneNumberCantEmpty);
        }
    }
}