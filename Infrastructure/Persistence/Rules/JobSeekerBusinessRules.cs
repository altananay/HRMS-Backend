using Application.Repositories;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class JobSeekerBusinessRules
    {
        private readonly IJobSeekerReadRepository _jobSeekerReadRepository;

        public JobSeekerBusinessRules(IJobSeekerReadRepository jobSeekerReadRepository)
        {
            _jobSeekerReadRepository = jobSeekerReadRepository;
        }

        public void NationalityIdExists(string nationalityId)
        {
            var result = _jobSeekerReadRepository.Get(js => js.NationalityId == nationalityId);
            if (result != null)
            {
                throw new BusinessException(Messages.Mernis.NationalityIdExists);
            }
        }

        public void CheckIfJobSeekerExists(string id)
        {
            if (_jobSeekerReadRepository.GetById(id) == null)
            {
                throw new BusinessException(Messages.JobSeeker.JobSeekerNotExists);
            }
        }

        public void CheckIfJobSeekerExistsByEmail(string email)
        {
            if (_jobSeekerReadRepository.Get(e => e.Email == email) == null)
            {
                throw new BusinessException(Messages.JobSeeker.JobSeekerEmailNotExists);
            }
        }
    }
}