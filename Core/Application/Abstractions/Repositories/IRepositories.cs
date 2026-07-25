using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Application.Abstractions;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// Commits everything tracked in the current scope in one transaction.
    /// </summary>
    /// <remarks>
    /// The old repositories had no unit of work at all: each write was an independent Mongo call, so
    /// <c>JobSeekerManager.Delete</c> could remove the user document and then fail to remove the
    /// seeker, leaving the two permanently inconsistent. A single SaveChanges makes that atomic.
    /// </remarks>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Per-aggregate repositories with intention-revealing methods.
    /// </summary>
    /// <remarks>
    /// Deliberately not generic CRUD repositories, and deliberately not a raw
    /// <c>IApplicationDbContext</c> exposing DbSets.
    ///
    /// The generic trio that used to live here (<c>IReadRepository</c>/<c>IWriteRepository</c>/
    /// <c>IDeleteRepository</c>, 30 near-empty marker interfaces plus 34 implementations) leaked the
    /// provider anyway — <c>IRepository&lt;T&gt;.collection</c> returned <c>IMongoCollection&lt;T&gt;</c>
    /// straight into the Application layer — so it abstracted nothing.
    ///
    /// Exposing DbSets instead would drag <c>Microsoft.EntityFrameworkCore</c> into Application and
    /// make handlers effectively un-unit-testable, since faking a DbSet's async query provider is
    /// miserable and the resulting test asserts LINQ-to-Objects semantics that differ from SQL.
    /// Small interfaces like these are one line to substitute with NSubstitute, and keep Application
    /// free of any persistence package.
    /// </remarks>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<PagedResult<User>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        void Remove(User user);

        /// <summary>
        /// Reads just the security stamp and active flag, for per-request token validation.
        /// </summary>
        /// <remarks>
        /// Projected to two scalar columns rather than materializing a <c>User</c>. Under
        /// table-per-type, loading the entity makes EF LEFT JOIN <c>job_seekers</c>,
        /// <c>employers</c> and <c>system_staff</c> to work out the concrete type — on a query that
        /// runs for every authenticated request. Projecting keeps it a single-table read.
        /// </remarks>
        Task<UserSecurityState?> GetSecurityStateAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Loads a user together with the role names needed to mint a token.</summary>
        Task<(User User, IReadOnlyList<string> Roles)?> GetForAuthenticationAsync(
            string email, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="GetForAuthenticationAsync(string, CancellationToken)"/>
        Task<(User User, IReadOnlyList<string> Roles)?> GetForAuthenticationByIdAsync(
            Guid userId, CancellationToken cancellationToken = default);
    }

    public interface IJobSeekerRepository
    {
        Task<JobSeeker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<JobSeeker?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<PagedResult<JobSeeker>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(JobSeeker jobSeeker);
        void Remove(JobSeeker jobSeeker);
    }

    public interface IEmployerRepository
    {
        Task<Employer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Employer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<PagedResult<Employer>> GetPagedAsync(PageRequest page, bool orderByHeadcount = false, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(Employer employer);
        void Remove(Employer employer);
    }

    public interface ISystemStaffRepository
    {
        Task<SystemStaff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SystemStaff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<PagedResult<SystemStaff>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        void Add(SystemStaff systemStaff);
        void Remove(SystemStaff systemStaff);
    }

    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        void AddUserRole(UserRole userRole);
    }

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        void Add(RefreshToken token);
        Task RevokeAllForUserAsync(Guid userId, DateTime utcNow, string? ip, CancellationToken cancellationToken = default);
        Task DeleteExpiredForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);
    }

    public interface ICvRepository
    {
        /// <summary>Loads the full aggregate — educations, experiences, languages, projects, files.</summary>
        Task<Cv?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Cv?> GetByJobSeekerIdAsync(Guid jobSeekerId, CancellationToken cancellationToken = default);
        Task<bool> ExistsForJobSeekerAsync(Guid jobSeekerId, CancellationToken cancellationToken = default);
        Task<PagedResult<Cv>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        void Add(Cv cv);
        void Remove(Cv cv);
    }

    public interface ICvFileRepository
    {
        Task<CvFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Includes the parent CV, whose <c>JobSeekerId</c> drives the download check.</summary>
        Task<CvFile?> GetByIdWithCvAsync(Guid id, CancellationToken cancellationToken = default);

        void AddRange(IEnumerable<CvFile> files);
        void Remove(CvFile file);
    }

    public interface IJobPositionRepository
    {
        Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<JobPosition?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the existing position with this name or creates one.
        /// </summary>
        /// <remarks>
        /// This is what makes job_positions a genuine shared lookup. Previously
        /// <c>JobAdvertisementManager.Add</c> inserted a brand-new position per advertisement.
        /// </remarks>
        Task<JobPosition> ResolveOrCreateAsync(string name, CancellationToken cancellationToken = default);

        Task<PagedResult<JobPosition>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken = default);
        void Remove(JobPosition position);
    }

    public interface IJobAdvertisementRepository
    {
        Task<JobAdvertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<JobAdvertisement?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PagedResult<JobAdvertisement>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            bool? isActive = null,
            bool orderByHighestSalary = false,
            CancellationToken cancellationToken = default);

        void Add(JobAdvertisement advertisement);
        void Remove(JobAdvertisement advertisement);
    }

    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<JobApplication?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Backs the unique (seeker, advertisement) rule with a cheap pre-check.</summary>
        Task<bool> ExistsForSeekerAndAdvertisementAsync(Guid jobSeekerId, Guid jobAdvertisementId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Whether this seeker has applied to any advertisement belonging to this employer.
        /// </summary>
        /// <remarks>
        /// The authorization predicate for CV downloads: an employer earns the right to read a CV by
        /// receiving an application from its owner, not by holding the employer role.
        /// </remarks>
        Task<bool> ExistsForEmployerAndSeekerAsync(Guid employerId, Guid jobSeekerId, CancellationToken cancellationToken = default);

        Task<PagedResult<JobApplication>> GetPagedAsync(
            PageRequest page,
            Guid? employerId = null,
            Guid? jobSeekerId = null,
            JobApplicationStatus? status = null,
            CancellationToken cancellationToken = default);

        void Add(JobApplication application);
        void Remove(JobApplication application);
    }

    public interface IContactRepository
    {
        Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Contact>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        void Add(Contact contact);
        void Remove(Contact contact);
    }
}
