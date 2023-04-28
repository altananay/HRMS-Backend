using Application.Constants;
using Domain.Objects;
using FluentValidation;

namespace Application.CrossCuttingConcerns.Validation.Validators.Cvs
{
    public class SocialMediaValidator : AbstractValidator<SocialMedia>
    {
        public SocialMediaValidator()
        {
            RuleFor(sm => sm.Github).NotEmpty().WithMessage(ValidationMessages.GithubCantEmpty);
            RuleFor(sm => sm.Linkedin).NotEmpty().WithMessage(ValidationMessages.LinkedinCantEmpty);
        }
    }
}