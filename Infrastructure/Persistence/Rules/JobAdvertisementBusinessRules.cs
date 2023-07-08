using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class JobAdvertisementBusinessRules
    {
        private readonly IJobAdvertisementReadRepository _repository;

        public JobAdvertisementBusinessRules(IJobAdvertisementReadRepository repository)
        {
            _repository = repository;
        }

        public void JobAdvertisementExists(string id)
        {
            var result = _repository.Get(e => e.Id == id);
            if (result == null)
            {
                throw new BusinessException(Messages.JobAdvertisement.JobAdvertisementNotExists);
            }
        }
    }
}