using Domain.Common;

namespace Application.Dtos
{
    public class SystemStaffForRegisterDto : IDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Status { get; set; }
        public string[] Claims { get; set; }
    }
}