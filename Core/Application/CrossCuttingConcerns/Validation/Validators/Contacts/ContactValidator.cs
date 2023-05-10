using Application.Constants;
using Application.Features.Contacts.Commands;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Contacts
{
    public class ContactValidator : AbstractValidator<CreateContactCommand>
    {
        public ContactValidator()
        {
            RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailCantEmpty);
            RuleFor(c => c.Message).MinimumLength(20).MaximumLength(200).NotEmpty().WithMessage(ValidationMessages.MessageLength);
            RuleFor(c => c.Subject).MinimumLength(5).MaximumLength(100).NotEmpty().WithMessage(ValidationMessages.SubjectLength);
            RuleFor(c => c.FirstName).MinimumLength(3).NotEmpty().WithMessage(ValidationMessages.FirstNameCantBeEmpty);
            RuleFor(c => c.LastName).MinimumLength(3).NotEmpty().WithMessage(ValidationMessages.LastNameCantBeEmpty);
        }
    }
}