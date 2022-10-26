using Application.Constants;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators
{
    public class LibraryAndFrameworkValidator : AbstractValidator<LibraryAndFramework>
    {
        public LibraryAndFrameworkValidator()
        {
            RuleFor(lf => lf.LibrariesAndFrameworks).NotEmpty().WithMessage(ValidationMessages.LibraryAndFrameworkCantEmpty);
        }
    }
}