using Application.Constants;
using Application.Features.JobSeekers.Commands;
using Application.Validators.CustomValidateRules;
using FluentValidation;

namespace Application.Validators.JobSeekers
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