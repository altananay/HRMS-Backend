using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Features.Auth.Commands;
using Application.Results;
using Application.Rules;
using Application.Utilities.Constants;
using Application.Utilities.JWT;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public sealed class AuthManager : IAuthService
    {
        /// <summary>
        /// A well-formed hash of a throwaway password, verified when no user matches.
        /// </summary>
        /// <remarks>
        /// Closes the timing half of the user-enumeration oracle. Returning early on an unknown email
        /// makes that path measurably faster than a wrong-password attempt, which is enough to
        /// enumerate registered addresses even when both return an identical 401. Verifying against a
        /// dummy hash keeps the work — and therefore the response time — comparable.
        ///
        /// The message half is closed separately: unknown email, wrong password and disabled account
        /// all return <see cref="Messages.Authentication.InvalidCredentials"/>. Previously an unknown
        /// email escaped as a BusinessException and surfaced as a 500 with a distinct message.
        /// </remarks>
        private static readonly string DummyPasswordHash =
            "AQAAAAIAAYagAAAAEL1sHqFhZ3vLbTLpJZ8pQwXn7yZKcC5vN1qYbGxHhTZ9kV2mR8sD4wA6fE1cP0uL9g==";

        private readonly IUserRepository _users;
        private readonly IJobSeekerRepository _jobSeekers;
        private readonly IEmployerRepository _employers;
        private readonly ISystemStaffRepository _systemStaff;
        private readonly IRoleRepository _roles;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUserSecurityStateProvider _securityState;
        private readonly ICurrentUserService _currentUser;
        private readonly BusinessRules _rules;
        private readonly TimeProvider _timeProvider;
        private readonly TokenOptions _tokenOptions;

        public AuthManager(
            IUserRepository users,
            IJobSeekerRepository jobSeekers,
            IEmployerRepository employers,
            ISystemStaffRepository systemStaff,
            IRoleRepository roles,
            IRefreshTokenRepository refreshTokens,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUserSecurityStateProvider securityState,
            ICurrentUserService currentUser,
            BusinessRules rules,
            TimeProvider timeProvider,
            IOptions<TokenOptions> tokenOptions)
        {
            _users = users;
            _jobSeekers = jobSeekers;
            _employers = employers;
            _systemStaff = systemStaff;
            _roles = roles;
            _refreshTokens = refreshTokens;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _securityState = securityState;
            _currentUser = currentUser;
            _rules = rules;
            _timeProvider = timeProvider;
            _tokenOptions = tokenOptions.Value;
        }

        // -----------------------------------------------------------------------------------------
        // Sign in
        // -----------------------------------------------------------------------------------------

        public async Task<IDataResult<AuthResponse>> LoginAsync(
            LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            var found = await _users.GetForAuthenticationAsync(command.Email, cancellationToken);

            if (found is null)
            {
                // Spend comparable time before failing — see DummyPasswordHash.
                _passwordHasher.Verify(DummyPasswordHash, command.Password);
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            var (user, roles) = found.Value;

            var outcome = _passwordHasher.Verify(user.PasswordHash, command.Password);

            if (outcome == PasswordVerificationOutcome.Failed)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            // Checked at last. Registration set this flag and nothing ever read it, so a deactivated
            // account could still sign in. Same generic message, so it is not an enumeration signal.
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            if (outcome == PasswordVerificationOutcome.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.Hash(command.Password);
            }

            // Housekeeping instead of a background service: cheap, and bounded per user.
            await _refreshTokens.DeleteExpiredForUserAsync(user.Id, UtcNow, cancellationToken);

            return new SuccessDataResult<AuthResponse>(
                await IssueTokensAsync(user, roles, cancellationToken),
                Messages.Authentication.LoggedIn);
        }

        // -----------------------------------------------------------------------------------------
        // Registration
        // -----------------------------------------------------------------------------------------

        public async Task<IDataResult<AuthResponse>> RegisterJobSeekerAsync(
            RegisterJobSeekerCommand command,
            CancellationToken cancellationToken = default)
        {
            await _rules.EnsureEmailIsAvailableAsync(command.Email, cancellationToken);

            var jobSeeker = new JobSeeker
            {
                Email = command.Email,
                PasswordHash = _passwordHasher.Hash(command.Password),
                FirstName = command.FirstName,
                LastName = command.LastName,
                NationalId = command.NationalId,
                DateOfBirth = command.DateOfBirth
            };

            _jobSeekers.Add(jobSeeker);
            await AssignRoleAsync(jobSeeker, Roles.JobSeeker, cancellationToken);

            // One SaveChanges for user + role. The old flow inserted an empty User document, copied
            // its id onto the seeker, then inserted the seeker — two writes, no transaction.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<AuthResponse>(
                await IssueTokensAsync(jobSeeker, [Roles.JobSeeker], cancellationToken),
                Messages.Authentication.Registered);
        }

        public async Task<IDataResult<AuthResponse>> RegisterEmployerAsync(
            RegisterEmployerCommand command,
            CancellationToken cancellationToken = default)
        {
            await _rules.EnsureEmailIsAvailableAsync(command.Email, cancellationToken);

            var employer = new Employer
            {
                Email = command.Email,
                PasswordHash = _passwordHasher.Hash(command.Password),
                CompanyName = command.CompanyName,
                CompanyPhone = command.CompanyPhone,
                WebSite = command.WebSite,
                NumberOfEmployees = command.NumberOfEmployees,
                Description = command.Description,
                Sectors = command.Sectors
            };

            _employers.Add(employer);
            await AssignRoleAsync(employer, Roles.Employer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<AuthResponse>(
                await IssueTokensAsync(employer, [Roles.Employer], cancellationToken),
                Messages.Authentication.Registered);
        }

        /// <remarks>
        /// Admin-only at the controller. The role is assigned here, server-side — never read from the
        /// request — which is what closes the old privilege-escalation path.
        /// </remarks>
        public async Task<IDataResult<CreatedDto>> RegisterSystemStaffAsync(
            RegisterSystemStaffCommand command,
            CancellationToken cancellationToken = default)
        {
            await _rules.EnsureEmailIsAvailableAsync(command.Email, cancellationToken);

            var staff = new SystemStaff
            {
                Email = command.Email,
                PasswordHash = _passwordHasher.Hash(command.Password),
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            _systemStaff.Add(staff);
            await AssignRoleAsync(staff, Roles.Admin, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessDataResult<CreatedDto>(new CreatedDto(staff.Id), Messages.SystemStaff.Added);
        }

        // -----------------------------------------------------------------------------------------
        // Token lifecycle
        // -----------------------------------------------------------------------------------------

        public async Task<IDataResult<AuthResponse>> RefreshAsync(
            RefreshTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            var hash = _tokenService.HashRefreshToken(command.RefreshToken);
            var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);

            if (stored is null)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidRefreshToken);
            }

            // Reuse detection. A token that was already rotated away should never be presented
            // again; if it is, the most likely explanation is that it was stolen. Revoking only that
            // token would leave whichever party holds the newer one — quite possibly the attacker —
            // with a live session, so the entire chain goes.
            if (stored.IsRevoked)
            {
                await RevokeEverythingAsync(stored.UserId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw new UnauthorizedAccessException(Messages.Authentication.InvalidRefreshToken);
            }

            if (stored.IsExpired(UtcNow))
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidRefreshToken);
            }

            var found = await _users.GetForAuthenticationByIdAsync(stored.UserId, cancellationToken);

            if (found is null || !found.Value.User.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidRefreshToken);
            }

            var (user, roles) = found.Value;

            // Rotate: the presented token dies and the replacement is linked to it, so a later replay
            // of this one is detectable by the branch above.
            stored.RevokedAt = UtcNow;
            stored.RevokedByIp = _currentUser.IpAddress;

            var response = await IssueTokensAsync(user, roles, cancellationToken, previous: stored);

            return new SuccessDataResult<AuthResponse>(response);
        }

        public async Task<IResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var stored = await _refreshTokens.GetByHashAsync(
                _tokenService.HashRefreshToken(refreshToken), cancellationToken);

            // Deliberately idempotent: an unknown or already-revoked token still reports success, so
            // logout cannot be used to probe which tokens exist.
            if (stored is not null && !stored.IsRevoked)
            {
                stored.RevokedAt = UtcNow;
                stored.RevokedByIp = _currentUser.IpAddress;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new SuccessResult(Messages.Authentication.LoggedOut);
        }

        public async Task<IResult> LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await RevokeEverythingAsync(userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Authentication.LoggedOut);
        }

        public async Task<IResult> ChangePasswordAsync(
            ChangePasswordCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);

            if (_passwordHasher.Verify(user.PasswordHash, command.CurrentPassword) == PasswordVerificationOutcome.Failed)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            user.PasswordHash = _passwordHasher.Hash(command.NewPassword);

            // Every existing session dies, including access tokens that have not expired yet. Without
            // the stamp being validated per request this would only take effect at the next refresh.
            await RevokeEverythingAsync(user.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Authentication.PasswordChanged);
        }

        public async Task<IDataResult<AuthenticatedUserDto>> GetCurrentUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException(Messages.Authentication.InvalidCredentials);

            var roles = await _roles.GetRoleNamesForUserAsync(userId, cancellationToken);

            return new SuccessDataResult<AuthenticatedUserDto>(ToDto(user, roles));
        }

        // -----------------------------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------------------------

        private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

        private async Task<AuthResponse> IssueTokensAsync(
            User user,
            IReadOnlyList<string> roles,
            CancellationToken cancellationToken,
            RefreshToken? previous = null)
        {
            var accessToken = _tokenService.CreateAccessToken(user, roles);
            var (refreshToken, refreshTokenHash) = _tokenService.CreateRefreshToken();

            var entity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = UtcNow.AddDays(_tokenOptions.RefreshTokenExpirationDays),
                CreatedByIp = _currentUser.IpAddress
            };

            _refreshTokens.Add(entity);

            if (previous is not null)
            {
                previous.ReplacedById = entity.Id;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                accessToken.Token,
                accessToken.ExpiresAtUtc,
                refreshToken,
                entity.ExpiresAt,
                ToDto(user, roles));
        }

        /// <summary>
        /// Kills every session for a user: revokes the refresh tokens and rotates the security stamp.
        /// </summary>
        /// <remarks>
        /// Both halves are required. Revoking refresh tokens alone leaves outstanding access tokens
        /// working until they expire — up to fifteen minutes during which a password change, a
        /// "log out everywhere", or a detected theft has visibly happened and yet changed nothing.
        /// Rotating the stamp is what makes those access tokens fail on their next request, and
        /// invalidating the cache is what makes it immediate rather than eventual.
        /// </remarks>
        private async Task RevokeEverythingAsync(Guid userId, CancellationToken cancellationToken)
        {
            await _refreshTokens.RevokeAllForUserAsync(userId, UtcNow, _currentUser.IpAddress, cancellationToken);

            var user = await _users.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
            {
                user.SecurityStamp = Guid.NewGuid();
            }

            _securityState.Invalidate(userId);
        }

        private async Task AssignRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var role = await _roles.GetByNameAsync(roleName, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Role '{roleName}' is missing. Roles are seeded at startup — see RoleSeeder.");

            _roles.AddUserRole(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        private static AuthenticatedUserDto ToDto(User user, IReadOnlyList<string> roles)
            => new(
                user.Id,
                user.Email,
                user switch
                {
                    JobSeeker seeker => $"{seeker.FirstName} {seeker.LastName}",
                    Employer employer => employer.CompanyName,
                    SystemStaff staff => $"{staff.FirstName} {staff.LastName}",
                    _ => user.Email
                },
                user.UserType,
                roles);
    }
}
