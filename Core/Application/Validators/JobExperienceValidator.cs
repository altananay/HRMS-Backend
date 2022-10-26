using Application.Constants;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators
{
    public class JobExperienceValidator : AbstractValidator<JobExperience>
    {
        public JobExperienceValidator()
        {
            RuleFor(je => je.Years).NotEmpty().WithMessage(ValidationMessages.YearsCantEmpty);
            RuleFor(je => je.LeaveWorkYear).NotEmpty().WithMessage(ValidationMessages.YearsCantEmpty);
            RuleFor(je => je.Position).NotEmpty().WithMessage(ValidationMessages.JobPositionCantEmpty);
            RuleFor(je => je.CompanyName).NotEmpty().WithMessage(ValidationMessages.CompanyNameInformationCantEmpty);
            RuleFor(je => je.Department).NotEmpty().WithMessage(ValidationMessages.DepartmentCantEmpty);
            RuleFor(je => je.Description).MinimumLength(10).MaximumLength(250).NotEmpty();
        }
    }
}