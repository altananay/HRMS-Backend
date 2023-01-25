using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Cvs.Queries.GetByJobSeekerIdCvQuery;

namespace Application.Features.Cvs.Queries
{
    public partial class GetByJobSeekerIdCvQuery : IRequest<GetByJobSeekerIdCvQueryResponse>
    {
        public string JobSeekerId { get; set; }

        public class GetByJobSeekerIdCvQueryResponse
        {
            public IDataResult<Cv> Cv;
        }

        public class GetByJobSeekerIdCvQueryHandler : IRequestHandler<GetByJobSeekerIdCvQuery, GetByJobSeekerIdCvQueryResponse>
        {
            private readonly ICVService _cvService;

            public GetByJobSeekerIdCvQueryHandler(ICVService cvService)
            {
                _cvService = cvService;
            }

            public async Task<GetByJobSeekerIdCvQueryResponse> Handle(GetByJobSeekerIdCvQuery request, CancellationToken cancellationToken)
            {
                var result = _cvService.GetByJobSeekerId(request.JobSeekerId);
                return new GetByJobSeekerIdCvQueryResponse { Cv = result};
            }
        }
    }
}