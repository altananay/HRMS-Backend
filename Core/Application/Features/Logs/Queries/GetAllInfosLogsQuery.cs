using Application.Abstractions;
using Application.CrossCuttingConcerns.Logging;
using Application.Results;
using MediatR;
using static Application.Features.Logs.Queries.GetAllInfosLogsQuery;

namespace Application.Features.Logs.Queries
{
    public partial class GetAllInfosLogsQuery : IRequest<GetAllInfosLogsQueryResponse>
    {
        public class GetAllInfosLogsQueryResponse
        {
            public IDataResult<IQueryable<GetAllInfosLogsDto>> Results { get; set; }
        }

        public class GetAllInfosLogsQueryHandler : IRequestHandler<GetAllInfosLogsQuery, GetAllInfosLogsQueryResponse>
        {
            private readonly ILogService _logService;

            public GetAllInfosLogsQueryHandler(ILogService logService)
            {
                _logService = logService;
            }

            public async Task<GetAllInfosLogsQueryResponse> Handle(GetAllInfosLogsQuery request, CancellationToken cancellationToken)
            {
                var logs = _logService.GetAllByInfo();
                return new GetAllInfosLogsQueryResponse { Results = logs };
            }
        }
    }
}