using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using MediatR;
using static Application.Features.JobApplications.Queries.GetResultByIdJobApplicationQuery;

namespace Application.Features.JobApplications.Queries
{
    public partial class GetResultByIdJobApplicationQuery : IRequest<GetResultByIdJobApplicationResponse>
    {
        public string Id { get; set; }

        public class GetResultByIdJobApplicationResponse
        {
            public IDataResult<GetJobApplicationResultDto> Result { get; set; }
        }

        public class GetResultByIdJobApplicationHandler : IRequestHandler<GetResultByIdJobApplicationQuery, GetResultByIdJobApplicationResponse>
        {
            private readonly IJobApplicationService _jobApplicationService;

            public GetResultByIdJobApplicationHandler(IJobApplicationService jobApplicationService)
            {
                _jobApplicationService = jobApplicationService;
            }

            public async Task<GetResultByIdJobApplicationResponse> Handle(GetResultByIdJobApplicationQuery request, CancellationToken cancellationToken)
            {
                var result = _jobApplicationService.GetResultById(request.Id);
                return new GetResultByIdJobApplicationResponse { Result = result };
            }
        }

    }
}