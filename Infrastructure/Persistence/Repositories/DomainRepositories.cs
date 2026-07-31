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

        // JobSeeker is not optional: the response projection reads the owner off it, so omitting the
        // include throws NullReferenceException rather than returning a CV with a missing name.
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

        public Task<PagedResult<Cv>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => WithDetails(_context.Cvs.AsNoTracking())
                .OrderByDescending(cv => cv.CreatedAt)
                // Split query + Skip/Take needs a total order, or rows can repeat across pages.
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
        private readonly TimeProvider _timeProvider;

        public JobPositionRepository(HrmsDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            // The insert below bypasses SaveChanges, so it also bypasses AuditingSaveChangesInterceptor
            // and has to stamp created_at itself — from the same clock, so tests that fake time still
            // see one consistent timeline.
            _timeProvider = timeProvider;
        }

        public Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobPositions.FirstOrDefaultAsync(position => position.Id == id, cancellationToken);

        public Task<JobPosition?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => _context.JobPositions.FirstOrDefaultAsync(position => position.Name == name, cancellationToken);

        /// <summary>
        /// Returns the position with this name, creating it if no one has yet.
        /// </summary>
        /// <remarks>
        /// The creating insert is executed **here**, not deferred to <c>SaveChanges</c>, and it is an
        /// upsert. Check-then-insert loses a race that the product actively invites: two employers
        /// publishing a posting with the same brand-new position name at the same moment both read
        /// "no such row", both queue an insert, and the second one hits
        /// <c>ix_job_positions_name</c> — a 23505 that surfaces as a 500 on an ordinary publish.
        /// <c>ON CONFLICT DO NOTHING</c> makes the create atomic, so the loser simply reads the
        /// winner's row back.
        /// <para>
        /// The trade-off is that the row is committed before the caller's own save. A job position is
        /// a lookup value with no owner, so an unreferenced one left behind by a later failure is
        /// inert — and the admin screen can delete it. Nothing else may follow this pattern: an
        /// entity that carries data must be written inside the caller's unit of work.
        /// </para>
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

            // `name` is citext, so the conflict target matches case-insensitively — exactly what
            // GetByNameAsync above compares with.
            await _context.Database.ExecuteSqlAsync(
                $"""
                 INSERT INTO job_positions (id, name, created_at)
                 VALUES ({Guid.CreateVersion7()}, {trimmed}, {_timeProvider.GetUtcNow().UtcDateTime})
                 ON CONFLICT (name) DO NOTHING
                 """,
                cancellationToken);

            // Read back rather than returning what was built: on a conflict the row that survived is
            // the other request's, and the caller needs *that* id for its foreign key.
            return await GetByNameAsync(trimmed, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Job position '{trimmed}' was neither inserted nor found immediately afterwards.");
        }

        public Task<PagedResult<JobPosition>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.JobPositions
                .AsNoTracking()
                .OrderBy(position => position.Name)
                .ToPagedResultAsync(page, cancellationToken);

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

        public Task<JobAdvertisement?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobAdvertisements
                .Include(advertisement => advertisement.Employer)
                .Include(advertisement => advertisement.JobPosition)
                .FirstOrDefaultAsync(advertisement => advertisement.Id == id, cancellationToken);

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobAdvertisements.AnyAsync(advertisement => advertisement.Id == id, cancellationToken);

        public Task<PagedResult<JobAdvertisement>> GetPagedAsync(
            PageRequest page,
            JobAdvertisementFilter filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.JobAdvertisements
                .AsNoTracking()
                .Include(advertisement => advertisement.Employer)
                .Include(advertisement => advertisement.JobPosition)
                .AsQueryable();

            if (filter.EmployerId is not null)
            {
                query = query.Where(advertisement => advertisement.EmployerId == filter.EmployerId);
            }

            if (filter.IsActive is not null)
            {
                query = query.Where(advertisement => advertisement.IsActive == filter.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.Skill))
            {
                // Array containment, so PostgreSQL can use the GIN index on skills rather than
                // unnesting every row.
                var skill = filter.Skill.Trim();
                query = query.Where(advertisement => advertisement.Skills.Contains(skill));
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                var city = filter.City.Trim();
                query = query.Where(advertisement => EF.Functions.ILike(advertisement.City!, city));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                // Escape the LIKE wildcards, or a search for "100%" matches everything.
                var term = $"%{Escape(filter.Search.Trim())}%";
                query = query.Where(advertisement =>
                    EF.Functions.ILike(advertisement.Title, term, LikeEscape)
                    || EF.Functions.ILike(advertisement.Description, term, LikeEscape));
            }

            query = filter.OrderByHighestSalary
                ? query.OrderByDescending(advertisement => advertisement.MaxSalary)
                : query.OrderByDescending(advertisement => advertisement.CreatedAt);

            return query.ToPagedResultAsync(page, cancellationToken);
        }

        private const string LikeEscape = "\\";

        private static string Escape(string term)
            => term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

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
            JobApplicationFilter filter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.JobApplications
                .AsNoTracking()
                .Include(application => application.JobAdvertisement)
                .Include(application => application.JobSeeker)
                .AsQueryable();

            if (filter.EmployerId is not null)
            {
                query = query.Where(application => application.JobAdvertisement.EmployerId == filter.EmployerId);
            }

            if (filter.JobSeekerId is not null)
            {
                query = query.Where(application => application.JobSeekerId == filter.JobSeekerId);
            }

            if (filter.JobAdvertisementId is not null)
            {
                query = query.Where(application => application.JobAdvertisementId == filter.JobAdvertisementId);
            }

            if (filter.Status is not null)
            {
                query = query.Where(application => application.Status == filter.Status);
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
