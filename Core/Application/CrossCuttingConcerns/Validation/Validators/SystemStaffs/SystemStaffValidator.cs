using Application.Constants;
using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Domain.Entities;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.SystemStaffs
{
    public class SystemStaffValidator : AbstractValidator<SystemStaff>
    {
        public SystemStaffValidator()
        {
            RuleFor(s => s.FirstName).NotEmpty().MinimumLength(3).WithMessage(ValidationMessages.FirstNameCantBeEmpty);
            RuleFor(s => s.LastName).NotEmpty().MinimumLength(3).WithMessage(ValidationMessages.LastNameCantBeEmpty);
            RuleFor(s => s.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleForEach(s => s.Claims).NotEmpty().Must(ClaimValidations.ClaimLengthValidate).WithMessage(ValidationMessages.ClaimCantBeEmpty);
            RuleFor(s => s.Claims).NotNull().Must(ClaimValidations.ClaimArrayValidate).WithMessage(ValidationMessages.ClaimFormat);
        }
    }
}