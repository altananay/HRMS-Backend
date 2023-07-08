using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class CvBusinessRules
    {
        private readonly ICvReadRepository _repository;

        public CvBusinessRules(ICvReadRepository repository)
        {
            _repository = repository;
        }

        public void CheckIfCvExists(string id)
        {
            if (_repository.GetById(id) == null)
            {
                throw new BusinessException(Messages.Cv.CvNotExists);
            }
        }

        public void CheckIfCvExistsByJobSeekerId(string id)
        {
            var values = _repository.Get(cv => cv.JobSeekerId == id);
            if (values != null)
            {
                throw new BusinessException(Messages.Cv.CvExists);
            }
        }
    }
}