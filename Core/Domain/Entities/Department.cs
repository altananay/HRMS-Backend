using Domain.Common;

namespace Domain.Entities
{
    public class Department : BaseEntity
    {
        public Guid EmployerId { get; set; }

        public Employer Employer { get; set; } = null!;

        public string Name { get; set; } = null!;

        public int? NumberOfEmployees { get; set; }
    }
}
