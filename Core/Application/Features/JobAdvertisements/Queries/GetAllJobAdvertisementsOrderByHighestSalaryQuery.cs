using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.JobAdvertisements.Queries.GetAllJobAdvertisementsOrderByHighestSalaryQuery;

namespace Application.Features.JobAdvertisements.Queries
{
    public partial class GetAllJobAdvertisementsOrderByHighestSalaryQuery : IRequest<GetAllJobAdvertisementsOrderByHighestSalaryQueryResponse>
    {
        public class GetAllJobAdvertisementsOrderByHighestSalaryQueryResponse
        {
            public IDataResult<IQueryable<JobAdvertisement>> JobAdvertisements;
        }

        public class GetAllOrderByHighestSalaryQueryHandler : IRequestHandler<GetAllJobAdvertisementsOrderByHighestSalaryQuery, GetAllJobAdvertisementsOrderByHighestSalaryQueryResponse>
        {
            private readonly IJobAdvertisementService _jobAdvertisementService;

            public GetAllOrderByHighestSalaryQueryHandler(IJobAdvertisementService jobAdvertisementService)
            {
                _jobAdvertisementService = jobAdvertisementService;
            }

            public async Task<GetAllJobAdvertisementsOrderByHighestSalaryQueryResponse> Handle(GetAllJobAdvertisementsOrderByHighestSalaryQuery request, CancellationToken cancellationToken)
            {
                var result = _jobAdvertisementService.GetAllByHighestSalary();
                return new GetAllJobAdvertisementsOrderByHighestSalaryQueryResponse { JobAdvertisements = result };
            }
        }
    }
}