using Application.Abstractions;
using Application.Results;
using Application.Utilities.Dtos;
using Domain.Entities;
using MediatR;
using static Application.Features.Employers.Queries.GetByIdEmployerQuery;

namespace Application.Features.Employers.Queries
{
    public partial class GetByIdEmployerQuery : IRequest<GetByIdEmployerQueryResponse>
    {
        public string Id { get; set; }

        public class GetByIdEmployerQueryResponse
        {
            public IDataResult<GetEmployerDto> Employer;
        }

        public class GetByIdEmployerQueryHandler : IRequestHandler<GetByIdEmployerQuery, GetByIdEmployerQueryResponse>
        {
            private readonly IEmployerService _employerService;

            public GetByIdEmployerQueryHandler(IEmployerService employerService)
            {
                _employerService = employerService;
            }

            public async Task<GetByIdEmployerQueryResponse> Handle(GetByIdEmployerQuery request, CancellationToken cancellationToken)
            {
                var result = _employerService.GetByEmployerIdWithFields(request.Id);
                return new GetByIdEmployerQueryResponse { Employer = result };
            }
        }
    }
}
