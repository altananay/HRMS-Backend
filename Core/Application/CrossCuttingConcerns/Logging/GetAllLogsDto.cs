using Domain.Common;
using Domain.Objects;

namespace Application.CrossCuttingConcerns.Logging
{
    public class GetAllLogsDto : IDto
    {
        public string Id { get; set; }
        public string Level { get; set; }
        public DateTime UtcTimeStamp { get; set; }
        public string RenderedMessage { get; set; }
        public Properties Properties { get; set; }
    }
}