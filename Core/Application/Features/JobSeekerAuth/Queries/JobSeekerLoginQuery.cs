using Application.Abstractions;
using Application.Results;
using Application.Utilities.JWT;
using Domain.Entities;
using MediatR;
using static Application.Features.Auth.Queries.JobSeekerLoginQuery;

namespace Application.Features.Auth.Queries
{
    public partial class JobSeekerLoginQuery : IRequest<JobSeekerLoginQueryResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public class JobSeekerLoginQueryResponse
        {
            public IDataResult<JobSeeker> Result { get; set; }
            public IDataResult<AccessToken> DataResult { get; set; }
        }

        public class JobSeekerLoginQueryHandler : IRequestHandler<JobSeekerLoginQuery, JobSeekerLoginQueryResponse>
        {
            private readonly IAuthService _authService;

            public JobSeekerLoginQueryHandler(IAuthService authService)
            {
                _authService = authService;
            }

            public async Task<JobSeekerLoginQueryResponse> Handle(JobSeekerLoginQuery request, CancellationToken cancellationToken)
            {
                var userToLogin = _authService.Login(request);
                if (!userToLogin.IsSuccess)
                {
                    return new JobSeekerLoginQueryResponse()
                    {
                        Result = userToLogin
                    };
                }

                var result = _authService.CreateAccessToken(userToLogin.Data);
                if (result.IsSuccess)
                {
                    return new JobSeekerLoginQueryResponse()
                    {
                        DataResult = result
                    };
                }

                return new JobSeekerLoginQueryResponse()
                {
                    DataResult = result
                };
            }
        }

    }
}