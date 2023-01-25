using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Queries.GetByIdJobAdvertisementQuery;

namespace Application.Features.JobAdvertisements.Queries
{
    public partial class GetByIdJobAdvertisementQuery : IRequest<GetByIdJobAdvertisementQueryResponse>
    {
        public string Id { get; set; }

        public class GetByIdJobAdvertisementQueryResponse
        {
            public IDataResult<JobAdvertisement> JobAdvertisement;
        }

        public class GetByIdJobAdvertisementQueryHandler : IRequestHandler<GetByIdJobAdvertisementQuery, GetByIdJobAdvertisementQueryResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public GetByIdJobAdvertisementQueryHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<GetByIdJobAdvertisementQueryResponse> Handle(GetByIdJobAdvertisementQuery request, CancellationToken cancellationToken)
            {
                var result = _jobAdvertisementService.GetById(request.Id);
                return new GetByIdJobAdvertisementQueryResponse { JobAdvertisement = result };
            }
        }
    }
}