using Domain.Objects;
using FluentValidation;

namespace Application.Validators.Cvs
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(p => p.ProjectName).NotEmpty().MinimumLength(5).MaximumLength(50);
            RuleFor(p => p.Description).NotEmpty().MinimumLength(25).MaximumLength(1000);
        }
    }
}