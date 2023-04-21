using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Employers.Queries.GetAllEmployerOrderByNumberOfEmployeesQuery;

namespace Application.Features.Employers.Queries
{
    public partial class GetAllEmployerOrderByNumberOfEmployeesQuery : IRequest<GetAllEmployerOrderByNumberOfEmployeesQueryResponse>
    {
        public class GetAllEmployerOrderByNumberOfEmployeesQueryResponse
        {
            public IDataResult<IQueryable<Employer>> Employers { get; set; }
        }

        public class GetAllEmployerOrderByNumberOfEmployeesQueryHandler : IRequestHandler<GetAllEmployerOrderByNumberOfEmployeesQuery, GetAllEmployerOrderByNumberOfEmployeesQueryResponse>
        {
            private readonly IEmployerService _employerService;

            public GetAllEmployerOrderByNumberOfEmployeesQueryHandler(IEmployerService employerService)
            {
                _employerService = employerService;
            }

            public async Task<GetAllEmployerOrderByNumberOfEmployeesQueryResponse> Handle(GetAllEmployerOrderByNumberOfEmployeesQuery request, CancellationToken cancellationToken)
            {
                var result = _employerService.GetAllByHighestNumberOfEmployees();
                return new GetAllEmployerOrderByNumberOfEmployeesQueryResponse { Employers = result };
            }
        }
    }
}