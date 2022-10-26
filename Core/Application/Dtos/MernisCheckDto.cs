using Domain.Common;

namespace Application.Dtos
{
    public class MernisCheckDto : IDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalityId { get; set; }
    }
}