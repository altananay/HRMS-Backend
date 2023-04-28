using Application.CrossCuttingConcerns.Logging;
using Domain.Entities;

namespace Application.Repositories
{
    public interface ILogReadRepository : IReadRepository<Log>
    {
        IQueryable<GetAllLogsDto> GetAllLogs();
        IQueryable<GetAllErrorLogsDto> GetAllErrorLogs();
        IQueryable<GetAllInfosLogsDto> GetAllInfosLogs();
    }
}