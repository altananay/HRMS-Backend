using Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules;
using Application.Features.JobApplications.Commands;
using Application.Utilities.Constants;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.JobApplications
{
    public class UpdateJobApplicationValidator : AbstractValidator<UpdateJobApplicationCommand>
    {
        public UpdateJobApplicationValidator()
        {
            RuleFor(j => j.JobApplicationId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.Result).NotEmpty().MinimumLength(10).MaximumLength(50).WithMessage(ValidationMessages.JobApplicationResultCantEmpty);
        }
    }
}