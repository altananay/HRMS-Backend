using Application.Abstractions;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;
using MediatR;
using static Application.Features.SystemStaffAuth.Queries.SystemStaffLoginQuery;

namespace Application.Features.SystemStaffAuth.Queries
{
    public partial class SystemStaffLoginQuery : IRequest<SystemStaffLoginQueryResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public class SystemStaffLoginQueryResponse
        {
            public IDataResult<SystemStaff> SystemStaff;
            public IDataResult<AccessToken> Token;
        }

        public class SystemStaffLoginQueryHandler : IRequestHandler<SystemStaffLoginQuery, SystemStaffLoginQueryResponse>
        {
            private readonly ISystemStaffAuthService _authService;

            public SystemStaffLoginQueryHandler(ISystemStaffAuthService authService)
            {
                _authService = authService;
            }

            public async Task<SystemStaffLoginQueryResponse> Handle(SystemStaffLoginQuery request, CancellationToken cancellationToken)
            {
                var loginAttempt = _authService.Login(request);
                if (!loginAttempt.IsSuccess)
                {
                    return new SystemStaffLoginQueryResponse { SystemStaff = loginAttempt };
                }
                SystemStaffLoginQueryResponse response = new();
                response.SystemStaff = loginAttempt;

                var result = _authService.CreateAccessToken(loginAttempt.Data);
                response.Token = result;
                return response;
            }
        }
    }
}