using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class EmployerBusinessRules
    {
        private readonly IEmployerReadRepository _repository;

        public EmployerBusinessRules(IEmployerReadRepository repository)
        {
            _repository = repository;
        }

        public void CheckIfEmployerExists(string id)
        {
            if (_repository.GetById(id) == null)
            {
                throw new BusinessException(Messages.Employer.EmployerNotExists); ;
            }
        }

        public void CheckIfEmployerExistsByEmail(string email)
        {
            if (_repository.Get(e => e.Email == email) == null)
            {
                throw new BusinessException(Messages.Employer.EmployerExists);
            }
        }
    }
}