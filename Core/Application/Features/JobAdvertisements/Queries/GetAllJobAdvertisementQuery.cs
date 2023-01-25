using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Queries.GetAllJobAdvertisementQuery;

namespace Application.Features.JobAdvertisements.Queries
{
    public partial class GetAllJobAdvertisementQuery : IRequest<GetAllJobAdvertisementQueryResponse>
    {
        public class GetAllJobAdvertisementQueryResponse
        {
            public IDataResult<IQueryable<JobAdvertisement>> JobAdvertisements;
        }

        public class GetAllJobAdvertisementQueryHandler : IRequestHandler<GetAllJobAdvertisementQuery, GetAllJobAdvertisementQueryResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public GetAllJobAdvertisementQueryHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<GetAllJobAdvertisementQueryResponse> Handle(GetAllJobAdvertisementQuery request, CancellationToken cancellationToken)
            {
                var result = _jobAdvertisementService.GetAll();
                return new GetAllJobAdvertisementQueryResponse { JobAdvertisements = result };
            }
        }
    }
}