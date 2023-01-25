using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Features.JobApplications.Queries.GetByIdJobApplicationQuery;

namespace Application.Features.JobApplications.Queries
{
    public partial class GetByIdJobApplicationQuery : IRequest<GetByIdJobApplicationQueryResponse>
    {
        public string Id { get; set; }

        public class GetByIdJobApplicationQueryResponse
        {
            public IDataResult<JobApplication> JobApplication;
        }

        public class GetByIdJobApplicationQueryHandler : IRequestHandler<GetByIdJobApplicationQuery, GetByIdJobApplicationQueryResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public GetByIdJobApplicationQueryHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }


            public async Task<GetByIdJobApplicationQueryResponse> Handle(GetByIdJobApplicationQuery request, CancellationToken cancellationToken)
            {
                var result = _jobApplicationService.GetById(request.Id);
                return new GetByIdJobApplicationQueryResponse { JobApplication = result };
            }
        }
    }
}