using Domain.Common;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Entities
{
    public class JobExperience
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string CompanyName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Years { get; set; }
        public string Description { get; set; }
        public string? LeaveWorkYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}