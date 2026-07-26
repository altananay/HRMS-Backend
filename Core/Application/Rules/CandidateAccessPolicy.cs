using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Exceptions;
using Application.Utilities.Constants;

namespace Application.Rules
{
    /// <summary>
    /// Decides who may read a candidate's personal data — their profile, their CV, and the files
    /// attached to it.
    /// </summary>
    /// <remarks>
    /// One rule, one place, three callers. It was previously a private method on
    /// <c>CvFileManager</c>, which meant only the file download enforced it: the CV itself and the
    /// seeker profile were readable by any authenticated caller who could name an id, so the
    /// carefully gated download proxy was handing out data that was already public to every
    /// signed-in user. An authorization rule that exists in one of the three places it applies is
    /// the shape most access-control bugs take, so it lives here and the managers ask.
    ///
    /// The employer clause is the reason this cannot be a role attribute: an employer earns the
    /// right to read a candidate by having received an application from them, which is a question
    /// about data, not about roles. <c>[Authorize(Roles = "employer")]</c> would grant every
    /// employer access to every candidate in the system.
    /// </remarks>
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

        /// <summary>
        /// Throws <see cref="ForbiddenException"/> unless <paramref name="requestedBy"/> is the
        /// candidate, an employer who has received an application from them, or an administrator.
        /// </summary>
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

        /// <summary>
        /// Throws unless <paramref name="requestedBy"/> is the candidate or an administrator.
        /// </summary>
        /// <remarks>
        /// Deliberately stricter than reading: an employer who received an application may look at a
        /// candidate, never modify or remove their records.
        /// </remarks>
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
