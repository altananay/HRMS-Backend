using Application.Abstractions;
using Application.CrossCuttingConcerns.Logging;
using Application.Results;
using MediatR;
using static Application.Features.Logs.Query.GetAllLogsQuery;

namespace Application.Features.Logs.Query
{
    public partial class GetAllLogsQuery : IRequest<GetAllQueryLogsResponse>
    {
        public class GetAllQueryLogsResponse
        {
            public IDataResult<IQueryable<GetAllLogsDto>> Results { get; set; }
        }

        public class GetAllQueryLogsHandler : IRequestHandler<GetAllLogsQuery, GetAllQueryLogsResponse>
        {
            private readonly ILogService _logService;

            public GetAllQueryLogsHandler(ILogService logService)
            {
                _logService = logService;
            }

            public async Task<GetAllQueryLogsResponse> Handle(GetAllLogsQuery request, CancellationToken cancellationToken)
            {
                var logs = _logService.GetAllLogs();
                return new GetAllQueryLogsResponse { Results = logs };
            }
        }

    }
}