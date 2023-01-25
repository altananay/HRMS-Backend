using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Employers.Queries.GetAllEmployerQuery;

namespace Application.Features.Employers.Queries
{
    public partial class GetAllEmployerQuery : IRequest<GetAllEmployerQueryResponse>
    {
        public class GetAllEmployerQueryResponse
        {
            public IDataResult<IQueryable<Employer>> Employers;
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
                var result = _employerService.GetAll();
                return new GetAllEmployerQueryResponse { Employers = result };
            }
        }
    }
}
