using Application.Abstractions.Services;
using Application.Common.Contracts;
using Application.Results;
using MediatR;

namespace Application.Features.Auth.Commands
{
    public sealed record AuthResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        AuthenticatedUserResponse User);

    public partial class LoginCommand : IRequest<LoginCommand.Response>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public sealed class Response
        {
            public IDataResult<AuthResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<LoginCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(LoginCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.LoginAsync(request, cancellationToken) };
        }
    }

    public partial class RegisterJobSeekerCommand : IRequest<RegisterJobSeekerCommand.Response>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? NationalId { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        public sealed class Response
        {
            public IDataResult<AuthResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<RegisterJobSeekerCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(RegisterJobSeekerCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.RegisterJobSeekerAsync(request, cancellationToken) };
        }
    }

    public partial class RegisterEmployerCommand : IRequest<RegisterEmployerCommand.Response>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? CompanyPhone { get; set; }
        public string? WebSite { get; set; }
        public int? NumberOfEmployees { get; set; }
        public string? Description { get; set; }
        public string[] Sectors { get; set; } = [];

        public sealed class Response
        {
            public IDataResult<AuthResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<RegisterEmployerCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(RegisterEmployerCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.RegisterEmployerAsync(request, cancellationToken) };
        }
    }

    public partial class RegisterSystemStaffCommand : IRequest<RegisterSystemStaffCommand.Response>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public sealed class Response
        {
            public IDataResult<CreatedResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<RegisterSystemStaffCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(RegisterSystemStaffCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.RegisterSystemStaffAsync(request, cancellationToken) };
        }
    }

    public partial class RefreshTokenCommand : IRequest<RefreshTokenCommand.Response>
    {
        public string RefreshToken { get; set; } = null!;

        public sealed class Response
        {
            public IDataResult<AuthResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<RefreshTokenCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.RefreshAsync(request, cancellationToken) };
        }
    }

    public partial class LogoutCommand : IRequest<LogoutCommand.Response>
    {
        public string RefreshToken { get; set; } = null!;

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<LogoutCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(LogoutCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.LogoutAsync(request.RefreshToken, cancellationToken) };
        }
    }

    public partial class LogoutAllCommand : IRequest<LogoutAllCommand.Response>
    {
        public Guid UserId { get; set; }

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<LogoutAllCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.LogoutAllAsync(request.UserId, cancellationToken) };
        }
    }

    public partial class ChangePasswordCommand : IRequest<ChangePasswordCommand.Response>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<ChangePasswordCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.ChangePasswordAsync(request, cancellationToken) };
        }
    }

    public partial class ForgotPasswordCommand : IRequest<ForgotPasswordCommand.Response>
    {
        public string Email { get; set; } = null!;

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<ForgotPasswordCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.ForgotPasswordAsync(request, cancellationToken) };
        }
    }

    public partial class ResetPasswordCommand : IRequest<ResetPasswordCommand.Response>
    {
        public string Token { get; set; } = null!;

        public string NewPassword { get; set; } = null!;

        public sealed class Response
        {
            public IResult Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<ResetPasswordCommand, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
                => new() { Result = await _authService.ResetPasswordAsync(request, cancellationToken) };
        }
    }
}

namespace Application.Features.Auth.Queries
{
    public partial class GetCurrentUserQuery : IRequest<GetCurrentUserQuery.Response>
    {
        public Guid UserId { get; set; }

        public sealed class Response
        {
            public IDataResult<AuthenticatedUserResponse> Result { get; init; } = null!;
        }

        public sealed class Handler : IRequestHandler<GetCurrentUserQuery, Response>
        {
            private readonly IAuthService _authService;

            public Handler(IAuthService authService) => _authService = authService;

            public async Task<Response> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
                => new() { Result = await _authService.GetCurrentUserAsync(request.UserId, cancellationToken) };
        }
    }
}
