using Application.Constants;
using Application.Features.JobSeekers.Commands;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.JobSeekers.Auth
{
    public class RegisterValidator : AbstractValidator<CreateJobSeekerCommand>
    {
        public RegisterValidator()
        {
            RuleFor(u => u.Email).NotEmpty().EmailAddress().WithMessage(ValidationMessages.EmailFormat);
            RuleFor(u => u.FirstName).NotEmpty();
            RuleFor(u => u.LastName).NotEmpty();
            RuleFor(u => u.Password).NotEmpty().MinimumLength(5).MaximumLength(10).WithMessage(ValidationMessages.PasswordCantEmpty);
        }
    }
}