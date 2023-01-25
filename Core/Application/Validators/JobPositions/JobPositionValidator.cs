using Application.Constants;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators.JobPositions
{
    public class JobPositionValidator : AbstractValidator<JobPosition>
    {
        public JobPositionValidator()
        {
            RuleFor(jp => jp.PositionName).NotEmpty().MinimumLength(5).WithMessage(ValidationMessages.JobPositionCantEmpty);
        }
    }
}