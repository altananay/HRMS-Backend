﻿using Application.Utilities.Constants;
using Domain.Objects;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Cvs
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