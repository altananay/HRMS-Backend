using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobSeekers.Queries.GetByIdJobSeekerQuery;

namespace Application.Features.JobSeekers.Queries
{
    public partial class GetByIdJobSeekerQuery : IRequest<GetByIdJobSeekerQueryResponse>
    {
        public string Id { get; set; }

        public class GetByIdJobSeekerQueryResponse
        {
            public IDataResult<JobSeeker> JobSeeker { get; set; }
        }

        public class GetByIdJobSeekerQueryHandler : IRequestHandler<GetByIdJobSeekerQuery, GetByIdJobSeekerQueryResponse>
        {
            private readonly IJobSeekerService _jobSeekerService;

            public GetByIdJobSeekerQueryHandler(IJobSeekerService jobSeekerService)
            {
                _jobSeekerService = jobSeekerService;
            }

            public async Task<GetByIdJobSeekerQueryResponse> Handle(GetByIdJobSeekerQuery request, CancellationToken cancellationToken)
            {
                var result = _jobSeekerService.GetById(request.Id);
                return new GetByIdJobSeekerQueryResponse { JobSeeker = result };
            }
        }
    }
}