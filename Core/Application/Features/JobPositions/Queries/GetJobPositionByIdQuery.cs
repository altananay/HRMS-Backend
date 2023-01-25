using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static Application.Features.JobPositions.Queries.GetJobPositionByIdQuery;

namespace Application.Features.JobPositions.Queries
{
    public partial class GetJobPositionByIdQuery : IRequest<GetJobPositionByIdQueryResponse>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public class GetJobPositionByIdQueryResponse
        {
            public IDataResult<JobPosition> JobPosition;
        }

        public class GetJobPositionByIdQueryHandler : IRequestHandler<GetJobPositionByIdQuery, GetJobPositionByIdQueryResponse>
        {
            private readonly IJobPositionService _jobPositionService;

            public GetJobPositionByIdQueryHandler(IJobPositionService jobPositionService)
            {
                _jobPositionService = jobPositionService;
            }

            public async Task<GetJobPositionByIdQueryResponse> Handle(GetJobPositionByIdQuery request, CancellationToken cancellationToken)
            {
                var result = _jobPositionService.GetById(request.Id);
                if (result.IsSuccess)
                {
                    return new GetJobPositionByIdQueryResponse { JobPosition = result };
                }
                return new GetJobPositionByIdQueryResponse { JobPosition = result };
            }
        }
    }
}