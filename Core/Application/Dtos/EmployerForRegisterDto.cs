using Domain.Common;

namespace Application.Dtos
{
    public class EmployerForRegisterDto : IDto
    {
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}