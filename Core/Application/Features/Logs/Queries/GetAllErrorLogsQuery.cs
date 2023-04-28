using Application.Abstractions;
using Application.CrossCuttingConcerns.Logging;
using Application.Results;
using MediatR;
using static Application.Features.Logs.Queries.GetAllErrorLogsQuery;

namespace Application.Features.Logs.Queries
{
    public partial class GetAllErrorLogsQuery : IRequest<GetAllErrorLogsQueryResponse>
    {
        public class GetAllErrorLogsQueryResponse
        {
            public IDataResult<IQueryable<GetAllErrorLogsDto>> Results { get; set; }
        }

        public class GetAllErrorLogsQueryHandler : IRequestHandler<GetAllErrorLogsQuery, GetAllErrorLogsQueryResponse>
        {
            private readonly ILogService _logService;

            public GetAllErrorLogsQueryHandler(ILogService logService)
            {
                _logService = logService;
            }

            public async Task<GetAllErrorLogsQueryResponse> Handle(GetAllErrorLogsQuery request, CancellationToken cancellationToken)
            {
                var logs = _logService.GetAllByError();
                return new GetAllErrorLogsQueryResponse { Results = logs };
            }
        }
    }
}