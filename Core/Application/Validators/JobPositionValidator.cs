using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class JobPositionValidator : AbstractValidator<JobPosition>
    {
        public JobPositionValidator()
        {
            RuleFor(jp => jp.PositionName).NotEmpty().MinimumLength(5);
        }
    }
}