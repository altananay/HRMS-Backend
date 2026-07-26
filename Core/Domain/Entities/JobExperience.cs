using Domain.Common;

namespace Domain.Entities
{
    public class JobExperience : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string CompanyName { get; set; } = null!;

        public string? Department { get; set; }

        public string Position { get; set; } = null!;

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        public string? Description { get; set; }
    }
}
