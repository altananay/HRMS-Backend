using Domain.Common;
using Domain.Objects;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class Log : BaseEntity
    {
        public string Level { get; set; }
        public DateTime UtcTimeStamp { get; set; }
        public string RenderedMessage { get; set; }
        public Properties Properties { get; set; }
    }
}