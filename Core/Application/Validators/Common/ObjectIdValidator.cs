using Application.Constants;
using Application.Validators.CustomValidateRules;
using FluentValidation;

namespace Application.Validators.Common
{
    public class ObjectIdValidator : AbstractValidator<string>
    {
        public ObjectIdValidator()
        {
            RuleFor(s => s).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
        }
    }
}