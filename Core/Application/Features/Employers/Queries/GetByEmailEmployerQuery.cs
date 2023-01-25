using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Employers.Queries.GetByEmailEmployerQuery;

namespace Application.Features.Employers.Queries
{
    public partial class GetByEmailEmployerQuery : IRequest<GetByEmailEmployerQueryResponse>
    {
        public string Email { get; set; }

        public class GetByEmailEmployerQueryResponse
        {
            public IDataResult<Employer> Employer;
        }

        public class GetByEmailEmployerQueryHandler : IRequestHandler<GetByEmailEmployerQuery, GetByEmailEmployerQueryResponse>
        {
            private readonly IEmployerService _employerService;

            public GetByEmailEmployerQueryHandler(IEmployerService employerService)
            {
                _employerService = employerService;
            }

            public async Task<GetByEmailEmployerQueryResponse> Handle(GetByEmailEmployerQuery request, CancellationToken cancellationToken)
            {
                var result = _employerService.GetByEmail(request.Email);
                return new GetByEmailEmployerQueryResponse { Employer = result };
            }
        }
    }
}