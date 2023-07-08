using Amazon.Runtime.Internal.Util;
using Application.Abstractions;
using Application.Results;
using Application.Utilities.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
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
