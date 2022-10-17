using Application.Dtos;
using FluentValidation;

namespace Application.Validators
{
    public class EmployerValidator : AbstractValidator<EmployerForRegisterDto>
    {
        public EmployerValidator()
        {
            RuleFor(e => e.CompanyName).NotEmpty().MinimumLength(5).WithMessage("Şirket adı boş bırakılamaz.");
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage("Email boş bırakılamaz.");
            RuleFor(e => e.WebSite).NotEmpty().WithMessage("Site ismi boş bırakılamaz.");
            RuleFor(e => e.CompanyPhone).NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.");
            RuleFor(e => e.Password).NotEmpty().MinimumLength(5).MaximumLength(10).WithMessage("Şifre zorunludur.");
        }
    }
}