using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;
using MediatR;
using static Application.Features.EmployerAuth.Queries.LoginQuery;

namespace Application.Features.EmployerAuth.Queries
{
    public partial class LoginQuery : IRequest<LoginQueryResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public class LoginQueryResponse
        {
            public IDataResult<Employer> Employer;
            public IDataResult<AccessToken> Token;
        }

        public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginQueryResponse>
        {
            private IEmployerAuthService _authService;

            public LoginQueryHandler(IEmployerAuthService authService)
            {
                _authService = authService;
            }

            public async Task<LoginQueryResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
            {
                var userToLogin = _authService.Login(request);
                if (!userToLogin.IsSuccess)
                {
                    return new LoginQueryResponse { Employer = userToLogin };
                }
                LoginQueryResponse loginQueryResponse = new();
                loginQueryResponse.Employer = userToLogin;

                var result = _authService.CreateAccessToken(userToLogin.Data);
                loginQueryResponse.Token = result;
                return loginQueryResponse;
            }
        }
    }
}
