using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class JobSeekerValidator : AbstractValidator<JobSeeker>
    {
        public JobSeekerValidator()
        {
            RuleFor(j => j.FirstName).NotEmpty();
            RuleFor(j => j.LastName).NotEmpty();
            RuleFor(j => j.Email).NotEmpty().EmailAddress();
            RuleFor(j => j.PasswordHash).NotEmpty();
        }
    }
}