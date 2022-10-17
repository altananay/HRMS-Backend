using Domain.Common;
using FluentValidation;
using MongoDB.Bson;

namespace Application.Validators
{
    public class DeleteValidator : AbstractValidator<string>
    {
        public DeleteValidator()
        {
            RuleFor(s => s).NotEmpty().Must(ObjectIdValidate).WithMessage("Bilgiyi doğru formatta girin");
        }

        private bool ObjectIdValidate(string arg)
        {

            if (ObjectId.TryParse(arg, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}