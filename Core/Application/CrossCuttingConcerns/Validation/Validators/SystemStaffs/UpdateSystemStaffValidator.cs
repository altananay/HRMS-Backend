using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Features.SystemStaffs.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.SystemStaffs
{
    public class UpdateSystemStaffValidator : AbstractValidator<UpdateSystemStaffCommand>
    {
        public UpdateSystemStaffValidator()
        {
            RuleFor(s => s.Id).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(s => s.FirstName).NotEmpty().MinimumLength(3).WithMessage(ValidationMessages.FirstNameCantBeEmpty);
            RuleFor(s => s.LastName).NotEmpty().MinimumLength(3).WithMessage(ValidationMessages.LastNameCantBeEmpty);
            RuleFor(s => s.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleForEach(s => s.Claims).NotEmpty().Must(ClaimValidations.ClaimLengthValidate).WithMessage(ValidationMessages.ClaimCantBeEmpty);
            RuleFor(s => s.Claims).NotNull().Must(ClaimValidations.ClaimArrayValidate).WithMessage(ValidationMessages.ClaimFormat);
        }
    }
}