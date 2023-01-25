using Application.Constants;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators.Cvs
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