using Application.Abstractions.Repositories;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public sealed class CvRepository : ICvRepository
    {
        private readonly HrmsDbContext _context;

        public CvRepository(HrmsDbContext context) => _context = context;

        /// <remarks>
        /// AsSplitQuery: five collection includes on one root would otherwise produce a cartesian
        /// explosion — 3 educations x 4 experiences x 2 languages is 24 duplicated rows before the
        /// projection even starts.
        /// </remarks>
        /// <summary>
        /// Every read of a CV carries its owner and its child collections.
        /// </summary>
        /// <remarks>
        /// <c>JobSeeker</c> is not optional here. <c>DomainMapper.ToDto(Cv)</c> reads the owner's
        /// name, email and date of birth off this navigation, so a query that omits it does not
        /// return a CV with a missing name — it throws NullReferenceException and the endpoint
        /// answers 500. Every CV read path did exactly that, including for the CV's own owner.
        /// </remarks>
        private static IQueryable<Cv> WithDetails(IQueryable<Cv> query)
            => query
                .Include(cv => cv.JobSeeker)
                .Include(cv => cv.Educations)
                .Include(cv => cv.JobExperiences)
                .Include(cv => cv.Languages)
                .Include(cv => cv.Projects)
                .Include(cv => cv.Files)
                .AsSplitQuery();

        public Task<Cv?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => WithDetails(_context.Cvs)
                .FirstOrDefaultAsync(cv => cv.Id == id, cancellationToken);

        public Task<Cv?> GetByJobSeekerIdAsync(Guid jobSeekerId, CancellationToken cancellationToken = default)
            => WithDetails(_context.Cvs)
                .FirstOrDefaultAsync(cv => cv.JobSeekerId == jobSeekerId, cancellationToken);

        public Task<bool> ExistsForJobSeekerAsync(Guid jobSeekerId, CancellationToken cancellationToken = default)
            => _context.Cvs.AnyAsync(cv => cv.JobSeekerId == jobSeekerId, cancellationToken);

        /// <remarks>
        /// The <c>ThenBy</c> is not decoration, and it is why this differs from the other paged
        /// queries. This is the only one that pairs a split query with Skip/Take, and a split query
        /// issues a separate SQL statement per collection: if two CVs share a CreatedAt the order is
        /// not total, the statements can disagree about which page a row belongs to, and a record
        /// surfaces twice or not at all. Guid v7 is time-ordered, so the id agrees with CreatedAt
        /// and simply makes the ordering unique.
        /// </remarks>
        public Task<PagedResult<Cv>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => WithDetails(_context.Cvs.AsNoTracking())
                .OrderByDescending(cv => cv.CreatedAt)
                .ThenByDescending(cv => cv.Id)
                .ToPagedResultAsync(page, cancellationToken);

        public void Add(Cv cv) => _context.Cvs.Add(cv);

        public void Remove(Cv cv) => _context.Cvs.Remove(cv);
    }

    public sealed class CvFileRepository : ICvFileRepository
    {
        private readonly HrmsDbContext _context;

        public CvFileRepository(HrmsDbContext context) => _context = context;

        public Task<CvFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.CvFiles.FirstOrDefaultAsync(file => file.Id == id, cancellationToken);

        public Task<CvFile?> GetByIdWithCvAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.CvFiles
                .Include(file => file.Cv)
                .FirstOrDefaultAsync(file => file.Id == id, cancellationToken);

        public void AddRange(IEnumerable<CvFile> files) => _context.CvFiles.AddRange(files);

        public void Remove(CvFile file) => _context.CvFiles.Remove(file);
    }

    public sealed class JobPositionRepository : IJobPositionRepository
    {
        private readonly HrmsDbContext _context;

        public JobPositionRepository(HrmsDbContext context) => _context = context;

        public Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobPositions.FirstOrDefaultAsync(position => position.Id == id, cancellationToken);

        public Task<JobPosition?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => _context.JobPositions.FirstOrDefaultAsync(position => position.Name == name, cancellationToken);

        /// <remarks>
        /// Also checks the change tracker, so two advertisements added in the same unit of work that
        /// name the same position reuse one instance instead of racing to insert duplicates and
        /// tripping the unique index.
        /// </remarks>
        public async Task<JobPosition> ResolveOrCreateAsync(string name, CancellationToken cancellationToken = default)
        {
            var trimmed = name.Trim();

            var pending = _context.ChangeTracker.Entries<JobPosition>()
                .Where(entry => entry.State == EntityState.Added)
                .Select(entry => entry.Entity)
                .FirstOrDefault(position => string.Equals(position.Name, trimmed, StringComparison.OrdinalIgnoreCase));

            if (pending is not null)
            {
                return pending;
            }

            var existing = await GetByNameAsync(trimmed, cancellationToken);
            if (existing is not null)
            {
                return existing;
            }

            var created = new JobPosition { Name = trimmed };
            _context.JobPositions.Add(created);
            return created;
        }

        public Task<PagedResult<JobPosition>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.JobPositions
                .AsNoTracking()
                .OrderBy(position => position.Name)
                .ToPagedResultAsync(page, cancellationToken);

        /// <summary>Lets the caller return 409 rather than let the RESTRICT foreign key throw.</summary>
        public Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobAdvertisements.AnyAsync(advertisement => advertisement.JobPositionId == id, cancellationToken);

        public void Remove(JobPosition position) => _context.JobPositions.Remove(position);
    }

    public sealed class JobAdvertisementRepository : IJobAdvertisementRepository
    {
        private readonly HrmsDbContext _context;

        public JobAdvertisementRepository(HrmsDbContext context) => _context = context;

        public Task<JobAdvertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobAdvertisements.FirstOrDefaultAsync(advertisement => advertisement.Id == id, cancellationToken);

        /// <remarks>
        /// Employer and JobPosition are included because the response DTO projects CompanyName,
        /// CompanyPhone, WebSite, Email and the position name from them — the five columns that used
        /// to be denormalized onto every advertisement row.
        /// </remarks>
        public Task<JobAdvertisement?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobAdvertisements
                .Include(advertisement => advertisement.Employer)
                .Include(advertisement => advertisement.JobPosition)
                .FirstOrDefaultAsync(advertisement => advertisement.Id == id, cancellationToken);

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobAdvertisements.AnyAsync(advertisement => advertisement.Id == id, cancellationToken);

        public Task<PagedResult<JobAdvertisement>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            bool? isActive = null,
            bool orderByHighestSalary = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.JobAdvertisements
                .AsNoTracking()
                .Include(advertisement => advertisement.Employer)
                .Include(advertisement => advertisement.JobPosition)
                .AsQueryable();

            if (employerId is not null)
            {
                query = query.Where(advertisement => advertisement.EmployerId == employerId);
            }

            if (isActive is not null)
            {
                query = query.Where(advertisement => advertisement.IsActive == isActive);
            }

            query = orderByHighestSalary
                ? query.OrderByDescending(advertisement => advertisement.MaxSalary)
                : query.OrderByDescending(advertisement => advertisement.CreatedAt);

            return query.ToPagedResultAsync(page, cancellationToken);
        }

        public void Add(JobAdvertisement advertisement) => _context.JobAdvertisements.Add(advertisement);

        public void Remove(JobAdvertisement advertisement) => _context.JobAdvertisements.Remove(advertisement);
    }

    public sealed class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly HrmsDbContext _context;

        public JobApplicationRepository(HrmsDbContext context) => _context = context;

        public Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobApplications.FirstOrDefaultAsync(application => application.Id == id, cancellationToken);

        public Task<JobApplication?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobApplications
                .Include(application => application.JobAdvertisement).ThenInclude(advertisement => advertisement.Employer)
                .Include(application => application.JobSeeker)
                .FirstOrDefaultAsync(application => application.Id == id, cancellationToken);

        public Task<bool> ExistsForSeekerAndAdvertisementAsync(
            Guid jobSeekerId,
            Guid jobAdvertisementId,
            CancellationToken cancellationToken = default)
            => _context.JobApplications.AnyAsync(
                application => application.JobSeekerId == jobSeekerId
                               && application.JobAdvertisementId == jobAdvertisementId,
                cancellationToken);

        public Task<bool> ExistsForEmployerAndSeekerAsync(
            Guid employerId,
            Guid jobSeekerId,
            CancellationToken cancellationToken = default)
            => _context.JobApplications.AnyAsync(
                application => application.JobSeekerId == jobSeekerId
                               && application.JobAdvertisement.EmployerId == employerId,
                cancellationToken);

        public Task<PagedResult<JobApplication>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            Guid? jobSeekerId = null,
            JobApplicationStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.JobApplications
                .AsNoTracking()
                .Include(application => application.JobAdvertisement)
                .Include(application => application.JobSeeker)
                .AsQueryable();

            // Reached through the advertisement rather than a denormalized EmployerId column,
            // which is why that column could be dropped.
            if (employerId is not null)
            {
                query = query.Where(application => application.JobAdvertisement.EmployerId == employerId);
            }

            if (jobSeekerId is not null)
            {
                query = query.Where(application => application.JobSeekerId == jobSeekerId);
            }

            if (status is not null)
            {
                query = query.Where(application => application.Status == status);
            }

            return query
                .OrderByDescending(application => application.CreatedAt)
                .ToPagedResultAsync(page, cancellationToken);
        }

        public void Add(JobApplication application) => _context.JobApplications.Add(application);

        public void Remove(JobApplication application) => _context.JobApplications.Remove(application);
    }

    public sealed class ContactRepository : IContactRepository
    {
        private readonly HrmsDbContext _context;

        public ContactRepository(HrmsDbContext context) => _context = context;

        public Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Contacts.FirstOrDefaultAsync(contact => contact.Id == id, cancellationToken);

        public Task<PagedResult<Contact>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.Contacts
                .AsNoTracking()
                .OrderByDescending(contact => contact.CreatedAt)
                .ToPagedResultAsync(page, cancellationToken);

        public void Add(Contact contact) => _context.Contacts.Add(contact);

        public void Remove(Contact contact) => _context.Contacts.Remove(contact);
    }
}
