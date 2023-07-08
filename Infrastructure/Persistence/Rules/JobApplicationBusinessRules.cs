using Application.Utilities.Constants;
using Application.Utilities.Exceptions;
using Persistence.Repositories;

namespace Persistence.Rules
{
    public class JobApplicationBusinessRules
    {
        private readonly JobAdvertisementBusinessRules _businessRules;
        private readonly JobSeekerBusinessRules _jobSeekerBusinessRules;
        private readonly JobApplicationReadRepository _applicationReadRepository;
        private readonly EmployerBusinessRules _employerBusinessRules;

        public JobApplicationBusinessRules(JobAdvertisementBusinessRules businessRules, JobSeekerBusinessRules jobSeekerBusinessRules, JobApplicationReadRepository jobApplicationReadRepository, EmployerBusinessRules employerBusinessRules)
        {
            _businessRules = businessRules;
            _jobSeekerBusinessRules = jobSeekerBusinessRules;
            _applicationReadRepository = jobApplicationReadRepository;
            _employerBusinessRules = employerBusinessRules;
        }

        public void CheckIfJobAdvertisementExists(string id)
        {
            _businessRules.JobAdvertisementExists(id);
        }

        public void CheckIfJobSeekerExists(string id)
        {
            _jobSeekerBusinessRules.CheckIfJobSeekerExists(id);
        }

        public void CheckIfEmployerExists(string id)
        {
            _employerBusinessRules.CheckIfEmployerExists(id);
        }

        public void CheckIfJobApplicationExists(string id)
        {
            if (_applicationReadRepository.GetById(id) == null)
            {
                throw new BusinessException(Messages.JobApplication.JobApplicationNotFound);
            }
        }
    }
}