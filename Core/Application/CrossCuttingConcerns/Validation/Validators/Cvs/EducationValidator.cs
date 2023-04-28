using Application.Constants;
using Domain.Objects;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Cvs
{
    public class EducationValidator : AbstractValidator<Education>
    {
        public EducationValidator()
        {
            RuleFor(e => e.School).NotEmpty().WithMessage(ValidationMessages.SchoolCantEmpty);
            RuleFor(e => e.Graduate).NotEmpty().WithMessage(ValidationMessages.GraduateCantEmpty);
            RuleFor(e => e.Years).NotEmpty().WithMessage(ValidationMessages.YearsCantEmpty);
            RuleFor(e => e.Major).NotEmpty().WithMessage(ValidationMessages.MajorCantEmpty);
        }
    }
}