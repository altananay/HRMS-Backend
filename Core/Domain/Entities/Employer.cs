using Domain.Enums;

namespace Domain.Entities
{
    public class Employer : User
    {
        public Employer() : base(UserType.Employer) { }

        public string CompanyName { get; set; } = null!;

        public string? CompanyPhone { get; set; }

        public string? WebSite { get; set; }

        public int? NumberOfEmployees { get; set; }

        public string? Description { get; set; }

        public string[] Sectors { get; set; } = [];

        public ICollection<Department> Departments { get; set; } = [];

        public ICollection<JobAdvertisement> JobAdvertisements { get; set; } = [];
    }
}
