using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobApplications.Queries.GetAllByJobSeekerIdJobApplicationQuery;

namespace Application.Features.JobApplications.Queries
{
    public partial class GetAllByJobSeekerIdJobApplicationQuery : IRequest<GetAllByJobSeekerIdJobApplicationQueryResponse>
    {
        public string Id { get; set; }

        public class GetAllByJobSeekerIdJobApplicationQueryResponse
        {
            public IDataResult<IQueryable<JobApplication>> JobApplications { get; set; }
        }

        public class GetAllByJobSeekerIdJobApplicationQueryHandler : IRequestHandler<GetAllByJobSeekerIdJobApplicationQuery, GetAllByJobSeekerIdJobApplicationQueryResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public GetAllByJobSeekerIdJobApplicationQueryHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<GetAllByJobSeekerIdJobApplicationQueryResponse> Handle(GetAllByJobSeekerIdJobApplicationQuery request, CancellationToken cancellationToken)
            {
                var results = _jobApplicationService.GetAllByJobSeekerId(request.Id);
                return new GetAllByJobSeekerIdJobApplicationQueryResponse { JobApplications = results };
            }
        }
    }
}