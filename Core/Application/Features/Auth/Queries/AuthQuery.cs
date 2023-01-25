using Application.Abstractions;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;
using MediatR;
using static Application.Features.Auth.Queries.AuthQuery;

namespace Application.Features.Auth.Queries
{
    public partial class AuthQuery : IRequest<AuthQueryResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public class AuthQueryResponse
        {
            public IDataResult<JobSeeker> Result { get; set; }
            public IDataResult<AccessToken> DataResult { get; set; }
        }

        public class AuthHandler : IRequestHandler<AuthQuery, AuthQueryResponse>
        {
            private readonly IAuthService _authService;

            public AuthHandler(IAuthService authService)
            {
                _authService = authService;
            }

            public async Task<AuthQueryResponse> Handle(AuthQuery request, CancellationToken cancellationToken)
            {
                var userToLogin = _authService.Login(request);
                if (!userToLogin.IsSuccess)
                {
                    return new AuthQueryResponse()
                    {
                        Result = userToLogin
                    };
                }

                var result = _authService.CreateAccessToken(userToLogin.Data);
                if (result.IsSuccess)
                {
                    return new AuthQueryResponse()
                    {
                        DataResult = result
                    };
                }

                return new AuthQueryResponse()
                {
                    DataResult = result
                };
            }
        }

    }
}