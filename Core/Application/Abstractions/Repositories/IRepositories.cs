using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Application.Abstractions;

namespace Application.Abstractions.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<PagedResult<User>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
        void Remove(User user);

        Task<UserSecurityState?> GetSecurityStateAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<(User User, IReadOnlyList<string> Roles)?> GetForAuthenticationAsync(
            string email, CancellationToken cancellationToken = default);

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

        /// <summary>Active employers only, alphabetical — the anonymous company directory.</summary>
        Task<PagedResult<Employer>> GetPublicPagedAsync(PageRequest page, CancellationToken cancellationToken = default);
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

    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        void Add(PasswordResetToken token);

        /// <summary>
        /// Marks every outstanding link for this user as used.
        /// </summary>
        /// <remarks>
        /// Called on a successful reset and when a new link is requested, so only the newest link is
        /// ever live. Without it, requesting a second link would leave the first one usable — and a
        /// user who requests a reset because they suspect compromise would still have a live token
        /// sitting in whichever mailbox the attacker can read.
        /// </remarks>
        Task InvalidateAllForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);
    }

    public interface ICvRepository
    {
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

        Task<CvFile?> GetByIdWithCvAsync(Guid id, CancellationToken cancellationToken = default);

        void AddRange(IEnumerable<CvFile> files);
        void Remove(CvFile file);
    }

    public interface IJobPositionRepository
    {
        Task<JobPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<JobPosition?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

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
            JobAdvertisementFilter filter,
            CancellationToken cancellationToken = default);

        void Add(JobAdvertisement advertisement);
        void Remove(JobAdvertisement advertisement);
    }

    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<JobApplication?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> ExistsForSeekerAndAdvertisementAsync(Guid jobSeekerId, Guid jobAdvertisementId, CancellationToken cancellationToken = default);

        Task<bool> ExistsForEmployerAndSeekerAsync(Guid employerId, Guid jobSeekerId, CancellationToken cancellationToken = default);

        Task<PagedResult<JobApplication>> GetPagedAsync(
            PageRequest page,
            JobApplicationFilter filter,
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
