using Application.CrossCuttingConcerns.Logging;
using Application.Results;

namespace Application.Abstractions
{
    public interface ILogService
    {
        IDataResult<IQueryable<GetAllLogsDto>> GetAllLogs();
        IDataResult<IQueryable<GetAllErrorLogsDto>> GetAllByError();
        IDataResult<IQueryable<GetAllInfosLogsDto>> GetAllByInfo();
    }
}