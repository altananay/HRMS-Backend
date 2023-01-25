using Application.Constants;
using Application.Features.JobAdvertisements.Commands;
using Application.Validators.CustomValidateRules;
using FluentValidation;

namespace Application.Validators.JobAdvertisements
{
    public class CreateJobAdvertisementValidator : AbstractValidator<CreateJobAdvertisementCommand>
    {
        public CreateJobAdvertisementValidator()
        {
            RuleFor(j => j.EmployerId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.JobPositionId).NotEmpty().Must(ObjectIdValidation.ObjectIdValidate).WithMessage(ValidationMessages.ObjectIdValidationError);
            RuleFor(j => j.Description).NotEmpty().MinimumLength(20).MaximumLength(1000);
            RuleFor(j => j.City).NotEmpty().MinimumLength(3).MaximumLength(20);
            RuleFor(j => j.Deadline).NotEmpty().WithMessage(ValidationMessages.DeadlineCantEmpty);
            RuleFor(j => j.JobType).NotEmpty().WithMessage(ValidationMessages.JobTypeCantEmpty);
            RuleFor(j => j.MinSalary).NotEmpty().WithMessage(ValidationMessages.MinSalaryCantEmpty);
            RuleFor(j => j.MinSalary).NotEmpty().LessThanOrEqualTo(j => j.MaxSalary).WithMessage(ValidationMessages.MinSalaryCantBeGreaterThanMaxSalary);
            RuleFor(j => j.MaxSalary).NotEmpty().GreaterThanOrEqualTo(j => j.MinSalary).WithMessage(ValidationMessages.MinSalaryCantBeGreaterThanMaxSalary);
            RuleFor(j => j.MaxSalary).NotEmpty().WithMessage(ValidationMessages.MaxSalaryCantEmpty);
            RuleFor(j => j.OpenPosition).NotEmpty().WithMessage(ValidationMessages.OpenPositionCantEmpty);
        }
    }
}
