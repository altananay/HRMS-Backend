using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobSeekers.Queries.GetByEmailJobSeekerQuery;

namespace Application.Features.JobSeekers.Queries
{
    public partial class GetByEmailJobSeekerQuery : IRequest<GetByEmailJobSeekerResponse>
    {
        public string Email { get; set; }

        public class GetByEmailJobSeekerResponse
        {
            public IDataResult<JobSeeker> JobSeeker;
        }

        public class GetByEmailJobSeekerHandler : IRequestHandler<GetByEmailJobSeekerQuery, GetByEmailJobSeekerResponse>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public GetByEmailJobSeekerHandler(IJobSeekerService jobSeekerService)
            {
                _jobSeekerService = jobSeekerService;
            }

            public async Task<GetByEmailJobSeekerResponse> Handle(GetByEmailJobSeekerQuery request, CancellationToken cancellationToken)
            {
                var result = _jobSeekerService.GetByMail(request.Email);
                return new GetByEmailJobSeekerResponse { JobSeeker = result };
            }
        }

    }
}