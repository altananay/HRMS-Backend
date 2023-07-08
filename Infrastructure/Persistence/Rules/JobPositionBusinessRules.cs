using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class JobPositionBusinessRules
    {
        private IJobPositionReadRepository _repository;

        public JobPositionBusinessRules(IJobPositionReadRepository repository)
        {
            _repository = repository;
        }

        public void CheckIfJobPositionExists(string id)
        {
            if (_repository.GetById(id) == null)
            {
                throw new BusinessException(Messages.JobPosition.JobPositionNotExists);
            }
        }

        public void CheckIfJobPositionExistsByName(string jobPosition)
        {
            if (_repository.Get(jp => jp.PositionName == jobPosition) != null)
            {
                throw new BusinessException(Messages.JobPosition.JobPositionExists);
            }
        }
    }
}