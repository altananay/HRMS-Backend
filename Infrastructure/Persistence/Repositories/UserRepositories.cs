using Application.Abstractions;
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

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
            => _context.Users.AnyAsync(user => user.Email == email, cancellationToken);

        public Task<PagedResult<User>> GetPagedAsync(PageRequest page, CancellationToken cancellationToken = default)
            => _context.Users
                .AsNoTracking()
                .OrderByDescending(user => user.CreatedAt)
                .ToPagedResultAsync(page, cancellationToken);

        public void Remove(User user) => _context.Users.Remove(user);

        public Task<UserSecurityState?> GetSecurityStateAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => _context.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => new UserSecurityState(user.SecurityStamp, user.IsActive))
                .FirstOrDefaultAsync(cancellationToken);

        public Task<(User User, IReadOnlyList<string> Roles)?> GetForAuthenticationAsync(
            string email,
            CancellationToken cancellationToken = default)
            => LoadWithRolesAsync(user => user.Email == email, cancellationToken);

        public Task<(User User, IReadOnlyList<string> Roles)?> GetForAuthenticationByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => LoadWithRolesAsync(user => user.Id == userId, cancellationToken);

        private async Task<(User User, IReadOnlyList<string> Roles)?> LoadWithRolesAsync(
            System.Linq.Expressions.Expression<Func<User, bool>> predicate,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(candidate => candidate.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .FirstOrDefaultAsync(predicate, cancellationToken);

            return user is null
                ? null
                : (user, user.UserRoles.Select(userRole => userRole.Role.Name).ToList());
        }
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

        /// <remarks>
        /// Departments are included because every caller needs them: the detail response projects them,
        /// and <c>EmployerManager.UpdateAsync</c> clears and rebuilds the collection. Without the
        /// Include the navigation is empty, so the clear did nothing, the rebuilt rows were never
        /// persisted and the update still answered 200 — and the public company page listed no
        /// departments for anyone. Covered by EmployerUpdateTests.
        /// </remarks>
        public Task<Employer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Employers
                .Include(employer => employer.Departments)
                .FirstOrDefaultAsync(employer => employer.Id == id, cancellationToken);

        public Task<Employer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _context.Employers.FirstOrDefaultAsync(employer => employer.Email == email, cancellationToken);

        public Task<PagedResult<Employer>> GetPagedAsync(
            PageRequest page,
            bool orderByHeadcount = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Employers.AsNoTracking();

            query = orderByHeadcount
                ? query.OrderByDescending(employer => employer.NumberOfEmployees)
                : query.OrderByDescending(employer => employer.CreatedAt);

            return query.ToPagedResultAsync(page, cancellationToken);
        }

        // Soft-deleted employers are already excluded by the query filter on User; the IsActive
        // predicate is what additionally keeps a suspended account out of the public directory.
        public Task<PagedResult<Employer>> GetPublicPagedAsync(
            PageRequest page,
            CancellationToken cancellationToken = default)
            => _context.Employers
                .AsNoTracking()
                .Where(employer => employer.IsActive)
                .OrderBy(employer => employer.CompanyName)
                .ThenBy(employer => employer.Id)
                .ToPagedResultAsync(page, cancellationToken);

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

        public Task DeleteExpiredForUserAsync(
            Guid userId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
            => _context.RefreshTokens
                .Where(token => token.UserId == userId && token.ExpiresAt < utcNow)
                .ExecuteDeleteAsync(cancellationToken);
    }

    public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly HrmsDbContext _context;

        public PasswordResetTokenRepository(HrmsDbContext context) => _context = context;

        public Task<PasswordResetToken?> GetByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
            => _context.PasswordResetTokens
                .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        public void Add(PasswordResetToken token) => _context.PasswordResetTokens.Add(token);

        public Task InvalidateAllForUserAsync(
            Guid userId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
            => _context.PasswordResetTokens
                .Where(token => token.UserId == userId && token.UsedAt == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(token => token.UsedAt, utcNow),
                    cancellationToken);
    }
}
