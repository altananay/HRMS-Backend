using Application.Abstractions.Repositories;
using Application.Common.Exceptions;
using Application.Utilities.Constants;
using Domain.Entities;

namespace Application.Rules
{
    public sealed class BusinessRules
    {
        private readonly IJobSeekerRepository _jobSeekers;
        private readonly IEmployerRepository _employers;
        private readonly ICvRepository _cvs;
        private readonly IJobAdvertisementRepository _advertisements;
        private readonly IJobApplicationRepository _applications;
        private readonly IJobPositionRepository _positions;
        private readonly IContactRepository _contacts;
        private readonly IUserRepository _users;

        public BusinessRules(
            IJobSeekerRepository jobSeekers,
            IEmployerRepository employers,
            ICvRepository cvs,
            IJobAdvertisementRepository advertisements,
            IJobApplicationRepository applications,
            IJobPositionRepository positions,
            IContactRepository contacts,
            IUserRepository users)
        {
            _jobSeekers = jobSeekers;
            _employers = employers;
            _cvs = cvs;
            _advertisements = advertisements;
            _applications = applications;
            _positions = positions;
            _contacts = contacts;
            _users = users;
        }

        public async Task<JobSeeker> EnsureJobSeekerExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _jobSeekers.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException(Messages.JobSeeker.NotFound);

        public async Task<Employer> EnsureEmployerExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _employers.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException(Messages.Employer.NotFound);

        public async Task<Cv> EnsureCvExistsForJobSeekerAsync(Guid jobSeekerId, CancellationToken cancellationToken = default)
            => await _cvs.GetByJobSeekerIdAsync(jobSeekerId, cancellationToken)
               ?? throw new NotFoundException(Messages.Cv.NotFound);

        public async Task EnsureCvDoesNotExistForJobSeekerAsync(Guid jobSeekerId, CancellationToken cancellationToken = default)
        {
            if (await _cvs.ExistsForJobSeekerAsync(jobSeekerId, cancellationToken))
            {
                throw new ConflictException(Messages.Cv.AlreadyExists);
            }
        }

        public async Task<JobAdvertisement> EnsureJobAdvertisementExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _advertisements.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException(Messages.JobAdvertisement.NotFound);

        public async Task<JobApplication> EnsureJobApplicationExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _applications.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException(Messages.JobApplication.NotFound);

        public async Task EnsureNotAlreadyAppliedAsync(
            Guid jobSeekerId,
            Guid jobAdvertisementId,
            CancellationToken cancellationToken = default)
        {
            if (await _applications.ExistsForSeekerAndAdvertisementAsync(jobSeekerId, jobAdvertisementId, cancellationToken))
            {
                throw new ConflictException(Messages.JobApplication.AlreadyApplied);
            }
        }

        public async Task<JobPosition> EnsureJobPositionExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _positions.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException(Messages.JobPosition.NotFound);

        public async Task EnsureJobPositionNotReferencedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (await _positions.IsReferencedAsync(id, cancellationToken))
            {
                throw new ConflictException(Messages.JobPosition.InUse);
            }
        }

        public async Task<Contact> EnsureContactExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _contacts.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException(Messages.Contact.NotFound);

        public async Task EnsureEmailIsAvailableAsync(string email, CancellationToken cancellationToken = default)
        {
            if (await _users.EmailExistsAsync(email, cancellationToken))
            {
                throw new ConflictException(Messages.Authentication.EmailAlreadyUsed);
            }
        }
    }
}
