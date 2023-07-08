using Application.Utilities.Constants;
using Domain.Objects;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Cvs
{
    public class HobbyValidator : AbstractValidator<Hobby>
    {
        public HobbyValidator()
        {
            RuleFor(h => h.Hobbies).NotEmpty().WithMessage(ValidationMessages.HobbiesCantEmpty);
        }
    }
}