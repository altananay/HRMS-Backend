using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Application.Dtos
{
    public class JobSeekerForUpdateDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        //public string Password { get; set; }

    }
}