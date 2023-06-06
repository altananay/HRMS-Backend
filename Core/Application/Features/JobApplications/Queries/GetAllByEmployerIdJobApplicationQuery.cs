using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobApplications.Queries.GetAllByEmployerIdJobApplicationQuery;

namespace Application.Features.JobApplications.Queries
{
    public partial class GetAllByEmployerIdJobApplicationQuery : IRequest<GetAllByEmployerIdJobApplicationQueryResponse>
    {
        public string Id { get; set; }
        
        public class GetAllByEmployerIdJobApplicationQueryResponse
        {
            public IDataResult<IQueryable<JobApplication>> JobApplications { get; set; }
        }

        public class GetAllByEmployerIdJobApplicationQueryHandler : IRequestHandler<GetAllByEmployerIdJobApplicationQuery, GetAllByEmployerIdJobApplicationQueryResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public GetAllByEmployerIdJobApplicationQueryHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<GetAllByEmployerIdJobApplicationQueryResponse> Handle(GetAllByEmployerIdJobApplicationQuery request, CancellationToken cancellationToken)
            {
                var results = _jobApplicationService.GetAllByEmployerId(request.Id);
                return new GetAllByEmployerIdJobApplicationQueryResponse { JobApplications = results };
            }
        }
    }
}