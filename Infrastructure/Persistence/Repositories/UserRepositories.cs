using Application.Abstractions.Repositories;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly HrmsDbContext _context;

        public UnitOfWork(HrmsDbContext context) => _context = context;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }

    public sealed class UserRepository : IUserRepository
    {
        private readonly HrmsDbContext _context;

        public UserRepository(HrmsDbContext context) => _context = context;

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        // Email is a citext column, so this comparison is case-insensitive in the database.
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
            => _context.Users.AnyAsync(user => user.Email == email, cancellationToken);

        public Task<PagedResult<User>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.Users
                .AsNoTracking()
                .OrderByDescending(user => user.CreatedAt)
                .ToPagedResultAsync(page, cancellationToken);

        // Rewritten to a soft delete by AuditingSaveChangesInterceptor.
        public void Remove(User user) => _context.Users.Remove(user);
    }

    public sealed class JobSeekerRepository : IJobSeekerRepository
    {
        private readonly HrmsDbContext _context;

        public JobSeekerRepository(HrmsDbContext context) => _context = context;

        public Task<JobSeeker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobSeekers.FirstOrDefaultAsync(seeker => seeker.Id == id, cancellationToken);

        public Task<JobSeeker?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.JobSeekers.FirstOrDefaultAsync(seeker => seeker.Email == email, cancellationToken);

        public Task<PagedResult<JobSeeker>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.JobSeekers
                .AsNoTracking()
                .OrderByDescending(seeker => seeker.CreatedAt)
                .ToPagedResultAsync(page, cancellationToken);

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.JobSeekers.AnyAsync(seeker => seeker.Id == id, cancellationToken);

        public void Add(JobSeeker jobSeeker) => _context.JobSeekers.Add(jobSeeker);

        public void Remove(JobSeeker jobSeeker) => _context.JobSeekers.Remove(jobSeeker);
    }

    public sealed class EmployerRepository : IEmployerRepository
    {
        private readonly HrmsDbContext _context;

        public EmployerRepository(HrmsDbContext context) => _context = context;

        public Task<Employer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Employers.FirstOrDefaultAsync(employer => employer.Id == id, cancellationToken);

        public Task<Employer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.Employers.FirstOrDefaultAsync(employer => employer.Email == email, cancellationToken);

        public Task<PagedResult<Employer>> GetPagedAsync(
            PageRequest page,
            bool orderByHeadcount = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Employers.AsNoTracking();

            // NumberOfEmployees is an int now, so this is a numeric sort. As a string it ordered
            // lexicographically, putting "9" after "100".
            query = orderByHeadcount
                ? query.OrderByDescending(employer => employer.NumberOfEmployees)
                : query.OrderByDescending(employer => employer.CreatedAt);

            return query.ToPagedResultAsync(page, cancellationToken);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Employers.AnyAsync(employer => employer.Id == id, cancellationToken);

        public void Add(Employer employer) => _context.Employers.Add(employer);

        public void Remove(Employer employer) => _context.Employers.Remove(employer);
    }

    public sealed class SystemStaffRepository : ISystemStaffRepository
    {
        private readonly HrmsDbContext _context;

        public SystemStaffRepository(HrmsDbContext context) => _context = context;

        public Task<SystemStaff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.SystemStaff.FirstOrDefaultAsync(staff => staff.Id == id, cancellationToken);

        public Task<SystemStaff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.SystemStaff.FirstOrDefaultAsync(staff => staff.Email == email, cancellationToken);

        public Task<PagedResult<SystemStaff>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.SystemStaff
                .AsNoTracking()
                .OrderByDescending(staff => staff.CreatedAt)
                .ToPagedResultAsync(page, cancellationToken);

        public void Add(SystemStaff systemStaff) => _context.SystemStaff.Add(systemStaff);

        public void Remove(SystemStaff systemStaff) => _context.SystemStaff.Remove(systemStaff);
    }

    public sealed class RoleRepository : IRoleRepository
    {
        private readonly HrmsDbContext _context;

        public RoleRepository(HrmsDbContext context) => _context = context;

        public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            => _context.Roles.FirstOrDefaultAsync(role => role.Name == name, cancellationToken);

        public async Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => await _context.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.UserId == userId)
                .Select(userRole => userRole.Role.Name)
                .ToListAsync(cancellationToken);

        public void AddUserRole(UserRole userRole) => _context.UserRoles.Add(userRole);
    }

    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly HrmsDbContext _context;

        public RefreshTokenRepository(HrmsDbContext context) => _context = context;

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => _context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => await _context.RefreshTokens
                .Where(token => token.UserId == userId && token.RevokedAt == null)
                .ToListAsync(cancellationToken);

        public void Add(RefreshToken token) => _context.RefreshTokens.Add(token);

        /// <summary>Bulk revoke — used by logout-all, password change and reuse detection.</summary>
        public Task RevokeAllForUserAsync(
            Guid userId,
            DateTime utcNow,
            string? ip,
            CancellationToken cancellationToken = default)
            => _context.RefreshTokens
                .Where(token => token.UserId == userId && token.RevokedAt == null)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(token => token.RevokedAt, utcNow)
                        .SetProperty(token => token.RevokedByIp, ip),
                    cancellationToken);

        /// <summary>Opportunistic cleanup on login, instead of a background service.</summary>
        public Task DeleteExpiredForUserAsync(
            Guid userId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
            => _context.RefreshTokens
                .Where(token => token.UserId == userId && token.ExpiresAt < utcNow)
                .ExecuteDeleteAsync(cancellationToken);
    }
}
