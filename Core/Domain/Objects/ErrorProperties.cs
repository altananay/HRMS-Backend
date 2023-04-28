using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Objects
{
    [BsonIgnoreExtraElements]
    public class ErrorProperties
    {
        public string SourceContext { get; set; }
        public string IpAddress { get; set; }
        public string RequestPath { get; set; }
    }
}