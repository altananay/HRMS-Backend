using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class SystemStaffBusinessRules
    {
        private ISystemStaffReadRepository _repository;

        public SystemStaffBusinessRules(ISystemStaffReadRepository repository)
        {
            _repository = repository;
        }

        public void CheckIfSystemStaffExists(string id)
        {
            if (_repository.GetById(id) == null)
                throw new BusinessException(Messages.SystemStaff.SystemStaffNotExists);
        }

        public void CheckIfSystemStaffExistsByEmail(string email)
        {
            if (_repository.Get(ss => ss.Email == email) == null)
            {
                throw new BusinessException(Messages.SystemStaff.SystemStaffNotExistsByEmail);
            }
        }
    }
}