using Domain.Enums;

namespace Domain.Entities
{
    public class Employer : User
    {
        public Employer() : base(UserType.Employer) { }

        public string CompanyName { get; set; } = null!;

        public string? CompanyPhone { get; set; }

        public string? WebSite { get; set; }

        /// <summary>
        /// Headcount.
        /// </summary>
        /// <remarks>
        /// Was a <c>string</c>, which made <c>GetAllByHighestNumberOfEmployees()</c> sort
        /// lexicographically — "9" ordered after "100".
        /// </remarks>
        public int? NumberOfEmployees { get; set; }

        public string? Description { get; set; }

        /// <summary>Free-text tags, mapped to a PostgreSQL <c>text[]</c> column.</summary>
        public string[] Sectors { get; set; } = [];

        public ICollection<Department> Departments { get; set; } = [];

        public ICollection<JobAdvertisement> JobAdvertisements { get; set; } = [];
    }
}
