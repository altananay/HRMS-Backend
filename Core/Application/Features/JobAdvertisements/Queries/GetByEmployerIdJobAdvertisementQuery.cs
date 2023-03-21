using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Queries.GetByEmployerIdJobAdvertisementQuery;

namespace Application.Features.JobAdvertisements.Queries
{
    public partial class GetByEmployerIdJobAdvertisementQuery : IRequest<GetByEmployerIdJobAdvertisementQueryResponse>
    {
        public string Id { get; set; }

        public class GetByEmployerIdJobAdvertisementQueryResponse
        {
            public IDataResult<IQueryable<JobAdvertisement>> JobAdvertisement;
        }

        public class GetByEmployerIdJobAdvertisementQueryHandler : IRequestHandler<GetByEmployerIdJobAdvertisementQuery, GetByEmployerIdJobAdvertisementQueryResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public GetByEmployerIdJobAdvertisementQueryHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<GetByEmployerIdJobAdvertisementQueryResponse> Handle(GetByEmployerIdJobAdvertisementQuery request, CancellationToken cancellationToken)
            {
                var result = _jobAdvertisementService.GetByEmployerId(request.Id);
                return new GetByEmployerIdJobAdvertisementQueryResponse { JobAdvertisement = result };
            }
        }
    }
}