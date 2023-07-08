using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Features.JobSeekers.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.JobSeekers
{
    public class UpdateJobSeekerValidator : AbstractValidator<UpdateJobSeekerCommand>
    {
        public UpdateJobSeekerValidator()
        {
            RuleFor(j => j.Id).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
        }
    }
}