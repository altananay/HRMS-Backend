using Application.Abstractions;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;
using MediatR;
using static Application.Features.EmployerAuth.Queries.EmployerLoginQuery;

namespace Application.Features.EmployerAuth.Queries
{
    public partial class EmployerLoginQuery : IRequest<EmployerLoginQueryResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public class EmployerLoginQueryResponse
        {
            public IDataResult<Employer> Employer;
            public IDataResult<AccessToken> Token;
        }

        public class EmployerLoginQueryHandler : IRequestHandler<EmployerLoginQuery, EmployerLoginQueryResponse>
        {
            private IEmployerAuthService _authService;

            public EmployerLoginQueryHandler(IEmployerAuthService authService)
            {
                _authService = authService;
            }

            public async Task<EmployerLoginQueryResponse> Handle(EmployerLoginQuery request, CancellationToken cancellationToken)
            {
                var userToLogin = _authService.Login(request);
                if (!userToLogin.IsSuccess)
                {
                    return new EmployerLoginQueryResponse { Employer = userToLogin };
                }
                EmployerLoginQueryResponse loginQueryResponse = new();
                loginQueryResponse.Employer = userToLogin;

                var result = _authService.CreateAccessToken(userToLogin.Data);
                loginQueryResponse.Token = result;
                return loginQueryResponse;
            }
        }
    }
}
