using Application.Abstractions;
using Application.Utilities.Constants;
using Application.Utilities.Exceptions;

namespace Persistence.Rules
{
    public class JobSeekerAuthBusinessRules
    {
        private readonly IJobSeekerService _jobSeekerService;

        public JobSeekerAuthBusinessRules(IJobSeekerService jobSeekerService)
        {
            _jobSeekerService = jobSeekerService;
        }

        public void UserExists(string email)
        {
            if (_jobSeekerService.GetByMail(email).Data != null)
            {
                throw new BusinessException(Messages.Authentication.UserAlreadyExists);
            }
        }
    }
}