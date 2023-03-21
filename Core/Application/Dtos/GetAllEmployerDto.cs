using Domain.Common;
using Domain.Entities;

namespace Application.Dtos
{
    public class GetAllEmployerDto : IDto
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public string[] Sectors { get; set; }
        public Department[] Departments { get; set; }
        public string NumberOfEmployees { get; set; }
        public string Description { get; set; }
    }
}