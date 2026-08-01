using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Exceptions;
using Application.Utilities.Constants;

namespace Application.Rules
{
    public sealed class CandidateAccessPolicy
    {
        private readonly IJobApplicationRepository _applications;
        private readonly ICurrentUserService _currentUser;

        public CandidateAccessPolicy(
            IJobApplicationRepository applications,
            ICurrentUserService currentUser)
        {
            _applications = applications;
            _currentUser = currentUser;
        }

        public async Task EnsureCanReadAsync(
            Guid jobSeekerId,
            Guid requestedBy,
            CancellationToken cancellationToken = default)
        {
            if (jobSeekerId == requestedBy || _currentUser.IsInRole(Roles.Admin))
            {
                return;
            }

            if (_currentUser.IsInRole(Roles.Employer)
                && await _applications.ExistsForEmployerAndSeekerAsync(requestedBy, jobSeekerId, cancellationToken))
            {
                return;
            }

            throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
        }

        public void EnsureCanModify(Guid jobSeekerId, Guid requestedBy)
        {
            if (jobSeekerId == requestedBy || _currentUser.IsInRole(Roles.Admin))
            {
                return;
            }

            throw new ForbiddenException(Messages.Authentication.AuthorizationDenied);
        }
    }
}
