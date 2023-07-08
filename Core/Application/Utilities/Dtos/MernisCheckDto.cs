using Domain.Common;

namespace Application.Utilities.Dtos
{
    public class MernisCheckDto : IDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalityId { get; set; }
    }
}