using Application.Constants;
using Application.Features.JobApplications.Commands;
using Application.Validators.CustomValidateRules;
using FluentValidation;

namespace Application.Validators.JobApplications
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