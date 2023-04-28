using Application.Abstractions;
using Application.Aspects;
using Application.CrossCuttingConcerns.Logging;
using Application.Repositories;
using Application.Results;

namespace Persistence.Concretes
{
    public class LogManager : ILogService
    {
        private readonly ILogReadRepository _logReadRepository;

        public LogManager(ILogReadRepository logReadRepository)
        {
            _logReadRepository = logReadRepository;
        }

        [SecuredOperation("admin")]

        public IDataResult<IQueryable<GetAllLogsDto>> GetAllLogs()
        {
            return new SuccessDataResult<IQueryable<GetAllLogsDto>>(_logReadRepository.GetAllLogs());
        }

        [SecuredOperation("admin")]
        public IDataResult<IQueryable<GetAllErrorLogsDto>> GetAllByError()
        {
            return new SuccessDataResult<IQueryable<GetAllErrorLogsDto>>(_logReadRepository.GetAllErrorLogs());
        }

        [SecuredOperation("admin")]
        public IDataResult<IQueryable<GetAllInfosLogsDto>> GetAllByInfo()
        {
            return new SuccessDataResult<IQueryable<GetAllInfosLogsDto>>(_logReadRepository.GetAllInfosLogs());
        }
    }
}