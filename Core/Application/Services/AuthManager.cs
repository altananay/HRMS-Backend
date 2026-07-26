using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Common.Contracts;
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

        public async Task<IDataResult<AuthResponse>> LoginAsync(
            LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            var found = await _users.GetForAuthenticationAsync(command.Email, cancellationToken);

            if (found is null)
            {
                _passwordHasher.Verify(DummyPasswordHash, command.Password);
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            var (user, roles) = found.Value;

            var outcome = _passwordHasher.Verify(user.PasswordHash, command.Password);

            if (outcome == PasswordVerificationOutcome.Failed)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Authentication.InvalidCredentials);
            }

            if (outcome == PasswordVerificationOutcome.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.Hash(command.Password);
            }

            await _refreshTokens.DeleteExpiredForUserAsync(user.Id, UtcNow, cancellationToken);

            return new SuccessDataResult<AuthResponse>(
                await IssueTokensAsync(user, roles, cancellationToken),
                Messages.Authentication.LoggedIn);
        }

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

        public async Task<IDataResult<CreatedResponse>> RegisterSystemStaffAsync(
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

            return new SuccessDataResult<CreatedResponse>(new CreatedResponse(staff.Id), Messages.SystemStaff.Added);
        }

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

            stored.RevokedAt = UtcNow;
            stored.RevokedByIp = _currentUser.IpAddress;

            var response = await IssueTokensAsync(user, roles, cancellationToken, previous: stored);

            return new SuccessDataResult<AuthResponse>(response);
        }

        public async Task<IResult> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var stored = await _refreshTokens.GetByHashAsync(
                _tokenService.HashRefreshToken(refreshToken), cancellationToken);

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

            await RevokeEverythingAsync(user.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult(Messages.Authentication.PasswordChanged);
        }

        public async Task<IDataResult<AuthenticatedUserResponse>> GetCurrentUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException(Messages.Authentication.InvalidCredentials);

            var roles = await _roles.GetRoleNamesForUserAsync(userId, cancellationToken);

            return new SuccessDataResult<AuthenticatedUserResponse>(ToResponse(user, roles));
        }

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
                ToResponse(user, roles));
        }

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

        private static AuthenticatedUserResponse ToResponse(User user, IReadOnlyList<string> roles)
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
