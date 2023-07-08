using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Common
{
    public class ObjectIdValidator : AbstractValidator<string>
    {
        public ObjectIdValidator()
        {
            RuleFor(s => s).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
        }
    }
}