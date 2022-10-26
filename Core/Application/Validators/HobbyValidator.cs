using Application.Constants;
using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class HobbyValidator : AbstractValidator<Hobby>
    {
        public HobbyValidator()
        {
            RuleFor(h => h.Hobbies).NotEmpty().WithMessage(ValidationMessages.HobbiesCantEmpty);
        }
    }
}