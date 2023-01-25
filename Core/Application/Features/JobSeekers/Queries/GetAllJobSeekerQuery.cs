using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobSeekers.Queries.GetAllJobSeekerQuery;

namespace Application.Features.JobSeekers.Queries
{
    public partial class GetAllJobSeekerQuery : IRequest<GetAllJobSeekerCommandResponse>
    {
        public class GetAllJobSeekerCommandResponse
        {
            public IDataResult<IQueryable<JobSeeker>> JobSeekers;
        }

        public class GetAllJobSeekerCommandHandler : IRequestHandler<GetAllJobSeekerQuery, GetAllJobSeekerCommandResponse>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public GetAllJobSeekerCommandHandler(IJobSeekerService jobSeekerService)
            {
                _jobSeekerService = jobSeekerService;
            }
            public async Task<GetAllJobSeekerCommandResponse> Handle(GetAllJobSeekerQuery request, CancellationToken cancellationToken)
            {
                var result = _jobSeekerService.GetAll();
                return new GetAllJobSeekerCommandResponse { JobSeekers = result };
            }
        }
    }
}
