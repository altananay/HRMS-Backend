using Application.Context;
using Application.CrossCuttingConcerns.Logging;
using Application.Repositories;
using Domain.Entities;
using Domain.Objects;
using MongoDB.Driver;

namespace Persistence.Repositories
{
    public class LogReadRepository : ReadRepository<Log>, ILogReadRepository
    {
        private readonly IMongoContext? _mongoContext;
        private readonly string? _collection;

        public LogReadRepository(IMongoContext mongoContext) : base(mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = typeof(Log).Name.ToLowerInvariant() + "s";
        }

        public IMongoCollection<Log> collection => _mongoContext.database.GetCollection<Log>(_collection);

        public IQueryable<GetAllLogsDto> GetAllLogs()
        {
            var result = from log in collection.AsQueryable()
                         select new GetAllLogsDto {Id = log.Id, Level = log.Level, Properties = log.Properties, RenderedMessage = log.RenderedMessage, UtcTimeStamp = log.UtcTimeStamp};
            return result;
        }

        public IQueryable<GetAllErrorLogsDto> GetAllErrorLogs()
        {
            List<GetAllErrorLogsDto> getAllErrorLogsDtos = new List<GetAllErrorLogsDto>();
            var results = from log in collection.AsQueryable()
                         where log.Level == "Error"
                         select new GetAllLogsDto { Id = log.Id, Level = log.Level, Properties = log.Properties, RenderedMessage = log.RenderedMessage, UtcTimeStamp = log.UtcTimeStamp };

            foreach (var result in results)
            {
                GetAllErrorLogsDto dto = new();
                ErrorProperties errorProperties = new();

                errorProperties.SourceContext = result.Properties.SourceContext;
                errorProperties.IpAddress = result.Properties.IpAddress;
                errorProperties.RequestPath = result.Properties.RequestPath;

                dto.Level = result.Level;
                dto.RenderedMessage = result.RenderedMessage;
                dto.Id = result.Id;
                dto.UtcTimeStamp = result.UtcTimeStamp;
                dto.Properties = errorProperties;
                getAllErrorLogsDtos.Add(dto);
            }
            return getAllErrorLogsDtos.AsQueryable();
        }

        public IQueryable<GetAllInfosLogsDto> GetAllInfosLogs()
        {
            var results = from log in collection.AsQueryable()
                          where log.Level == "Information"
                          select new GetAllInfosLogsDto { Id = log.Id, Level = log.Level, Properties = log.Properties, RenderedMessage = log.RenderedMessage, UtcTimeStamp = log.UtcTimeStamp };
            return results;
        }
    }
}