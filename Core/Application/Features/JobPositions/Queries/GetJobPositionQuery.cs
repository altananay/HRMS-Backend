using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobPositions.Queries.GetJobPositionQuery;

namespace Application.Features.JobPositions.Queries
{
    public partial class GetJobPositionQuery : IRequest<GetJobPositionQueryResponse>
    {
        public class GetJobPositionQueryResponse
        {
            public IDataResult<IQueryable<JobPosition>> jobPositions;
        }

        public class GetJobPositionQueryHandler : IRequestHandler<GetJobPositionQuery, GetJobPositionQueryResponse>
        {
            private readonly IJobPositionService _jobPositionService;

            public GetJobPositionQueryHandler(IJobPositionService jobPositionService)
            {
                _jobPositionService = jobPositionService;
            }

            public async Task<GetJobPositionQueryResponse> Handle(GetJobPositionQuery request, CancellationToken cancellationToken)
            {
                var result = _jobPositionService.GetAll();
                if (result.IsSuccess)
                {
                    return new GetJobPositionQueryResponse
                    {
                        jobPositions = result
                    };
                }

                return new GetJobPositionQueryResponse
                {
                    jobPositions = result
                };

            }
        }
    }
}