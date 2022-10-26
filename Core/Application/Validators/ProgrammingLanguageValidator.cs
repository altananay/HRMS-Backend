using Application.Constants;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators
{
    public class ProgrammingLanguageValidator : AbstractValidator<ProgrammingLanguage>
    {
        public ProgrammingLanguageValidator()
        {
            RuleFor(pl => pl.Languages).NotEmpty().WithMessage(ValidationMessages.ProgrammingLanguageCantEmpty);
            RuleFor(pl => pl.LanguageLevel).NotEmpty().WithMessage(ValidationMessages.LevelCantEmpty);
        }
    }
}