using Application.Abstractions;
using Application.Results;
using Domain.Entities;
using MediatR;
using static Application.Features.Cvs.Queries.GetAllCvQuery;

namespace Application.Features.Cvs.Queries
{
    public partial class GetAllCvQuery : IRequest<GetAllCvQueryResponse>
    {
        public class GetAllCvQueryResponse
        {
            public IDataResult<IQueryable<Cv>> Cvs;
        }

        public class GetAllCvQueryHandler : IRequestHandler<GetAllCvQuery, GetAllCvQueryResponse>
        {
            private readonly ICVService _cvService;

            public GetAllCvQueryHandler(ICVService cvService)
            {
                _cvService = cvService;
            }

            public async Task<GetAllCvQueryResponse> Handle(GetAllCvQuery request, CancellationToken cancellationToken)
            {
                var result = _cvService.GetAll();
                return new GetAllCvQueryResponse { Cvs = result };
            }
        }
    }
}