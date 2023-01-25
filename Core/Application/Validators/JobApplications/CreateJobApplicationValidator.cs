using Application.Constants;
using Application.Features.JobApplications.Commands;
using Application.Validators.CustomValidateRules;
using FluentValidation;

namespace Application.Validators.JobApplications
{
    public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationCommand>
    {
        public CreateJobApplicationValidator()
        {
            RuleFor(j => j.JobAdvertisementId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.JobSeekerId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.Result).NotEmpty().MinimumLength(10).MaximumLength(50).WithMessage(ValidationMessages.JobApplicationResultCantEmpty);        
        }

    }
}