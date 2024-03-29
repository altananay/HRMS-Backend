﻿using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Features.Contacts.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Contacts
{
    public class UpdateContactValidator : AbstractValidator<UpdateContactCommand>
    {
        public UpdateContactValidator()
        {
            RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailCantEmpty);
            RuleFor(c => c.Message).MinimumLength(20).MaximumLength(200).NotEmpty().WithMessage(ValidationMessages.MessageLength);
            RuleFor(c => c.Subject).MinimumLength(5).MaximumLength(100).NotEmpty().WithMessage(ValidationMessages.SubjectLength);
            RuleFor(c => c.FirstName).MinimumLength(3).NotEmpty().WithMessage(ValidationMessages.FirstNameCantBeEmpty);
            RuleFor(c => c.LastName).MinimumLength(3).NotEmpty().WithMessage(ValidationMessages.LastNameCantBeEmpty);
            RuleFor(c => c.Id).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
        }
    }
}