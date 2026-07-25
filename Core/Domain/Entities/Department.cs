using Domain.Common;

namespace Domain.Entities
{
    /// <summary>A department within an employer. Was an embedded array on the employer document.</summary>
    public class Department : BaseEntity
    {
        public Guid EmployerId { get; set; }

        public Employer Employer { get; set; } = null!;

        public string Name { get; set; } = null!;

        /// <summary>Was a <c>string</c>, for no reason and with the same lexicographic-sort problem.</summary>
        public int? NumberOfEmployees { get; set; }
    }
}
