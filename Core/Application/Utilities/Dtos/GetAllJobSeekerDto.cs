using Domain.Common;
using Domain.Entities;

namespace Application.Utilities.Dtos
{
    public class GetAllJobSeekerDto : IDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalityId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public Cv Cv { get; set; }
    }
}