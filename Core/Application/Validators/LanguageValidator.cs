using Application.Constants;
using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class LanguageValidator : AbstractValidator<Language>
    {
        public LanguageValidator()
        {
            RuleFor(l => l.Languages).NotEmpty().WithMessage(ValidationMessages.LanguagesCantEmpty);
            RuleFor(l => l.LanguageLevel).NotEmpty().WithMessage(ValidationMessages.LevelCantEmpty);
        }
    }
}