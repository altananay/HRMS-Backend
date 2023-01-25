using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobApplications.Queries.GetAllJobApplicationQuery;

namespace Application.Features.JobApplications.Queries
{
    public partial class GetAllJobApplicationQuery : IRequest<GetAllJobApplicationQueryResponse>
    {
        public class GetAllJobApplicationQueryResponse
        {
            public IDataResult<IQueryable<JobApplication>> JobApplications;
        }

        public class GetAllJobApplicationQueryHandler : IRequestHandler<GetAllJobApplicationQuery, GetAllJobApplicationQueryResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public GetAllJobApplicationQueryHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<GetAllJobApplicationQueryResponse> Handle(GetAllJobApplicationQuery request, CancellationToken cancellationToken)
            {
                var result = _jobApplicationService.GetAll();
                return new GetAllJobApplicationQueryResponse { JobApplications = result };
            }
        }
    }
}
