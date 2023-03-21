using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using MediatR;
using static Application.Features.Employers.Queries.GetAllEmployerQuery;

namespace Application.Features.Employers.Queries
{
    public partial class GetAllEmployerQuery : IRequest<GetAllEmployerQueryResponse>
    {
        public class GetAllEmployerQueryResponse
        {
            public IDataResult<IQueryable<GetAllEmployerDto>> Employers;
        }

        public class GetAllEmployerQueryHandler : IRequestHandler<GetAllEmployerQuery, GetAllEmployerQueryResponse>
        {
            private readonly IEmployerService _employerService;

            public GetAllEmployerQueryHandler(IEmployerService employerService)
            {
                _employerService = employerService;
            }

            public async Task<GetAllEmployerQueryResponse> Handle(GetAllEmployerQuery request, CancellationToken cancellationToken)
            {
                var result = _employerService.GetAllEmployer();
                return new GetAllEmployerQueryResponse { Employers = result };
            }
        }
    }
}
