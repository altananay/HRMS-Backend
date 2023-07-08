using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Features.JobApplications.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.JobApplications
{
    public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationCommand>
    {
        public CreateJobApplicationValidator()
        {
            RuleFor(j => j.JobAdvertisementId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.JobSeekerId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
        }

    }
}