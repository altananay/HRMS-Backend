using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(p => p.ProjectName).NotEmpty().MinimumLength(10).MaximumLength(50);
            RuleFor(p => p.Description).NotEmpty().MinimumLength(25).MaximumLength(250);
        }
    }
}