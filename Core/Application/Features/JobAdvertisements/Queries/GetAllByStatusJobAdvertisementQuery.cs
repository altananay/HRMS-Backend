using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Queries.GetAllByStatusJobAdvertisementQuery;

namespace Application.Features.JobAdvertisements.Queries
{
    public partial class GetAllByStatusJobAdvertisementQuery : IRequest<GetAllByStatusJobAdvertisementQueryResponse>
    {
        public bool Status { get; set; }

        public class GetAllByStatusJobAdvertisementQueryResponse
        {
            public IDataResult<IQueryable<JobAdvertisement>> JobAdvertisements;
        }

        public class GetAllByStatusJobAdvertisementQueryHandler : IRequestHandler<GetAllByStatusJobAdvertisementQuery, GetAllByStatusJobAdvertisementQueryResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public GetAllByStatusJobAdvertisementQueryHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }


            public async Task<GetAllByStatusJobAdvertisementQueryResponse> Handle(GetAllByStatusJobAdvertisementQuery request, CancellationToken cancellationToken)
            {
                var result = _jobAdvertisementService.GetAllByStatus(request.Status);
                return new GetAllByStatusJobAdvertisementQueryResponse { JobAdvertisements = result };
            }
        }

    }
}