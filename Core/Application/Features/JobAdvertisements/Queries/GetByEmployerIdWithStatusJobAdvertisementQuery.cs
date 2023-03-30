using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Queries.GetByEmployerIdWithStatusJobAdvertisementQuery;

namespace Application.Features.JobAdvertisements.Queries
{
    public partial class GetByEmployerIdWithStatusJobAdvertisementQuery : IRequest<GetByEmployerIdWithStatusJobAdvertisementQueryResponse>
    {
        public string Id { get; set; }
        public bool Status { get; set; }

        public class GetByEmployerIdWithStatusJobAdvertisementQueryResponse
        {
            public IDataResult<IQueryable<JobAdvertisement>> JobAdvertisement;
        }

        public class GetByEmployerIdWithStatusJobAdvertisementQueryHandler : IRequestHandler<GetByEmployerIdWithStatusJobAdvertisementQuery, GetByEmployerIdWithStatusJobAdvertisementQueryResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public GetByEmployerIdWithStatusJobAdvertisementQueryHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<GetByEmployerIdWithStatusJobAdvertisementQueryResponse> Handle(GetByEmployerIdWithStatusJobAdvertisementQuery request, CancellationToken cancellationToken)
            {
                var result = _jobAdvertisementService.GetByEmployerIdWithStatus(request.Id, request.Status);
                return new GetByEmployerIdWithStatusJobAdvertisementQueryResponse { JobAdvertisement = result };
            }
        }
    }
}