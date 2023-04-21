using Domain.Common;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class JobAdvertisement : BaseEntity
    {
        public string EmployerId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string JobPositionId { get; set; }
        public string JobPosition { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Experience { get; set; }
        public string[] Skills { get; set; }
        public string City { get; set; }
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal? MinSalary { get; set; }
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal? MaxSalary { get; set; }
        public string Currency { get; set; }
        public int OpenPosition { get; set; }
        public string JobType { get; set; }
        public DateTime Deadline { get; set; }
        public bool Status { get; set; }
    }
}