using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class JobAdvertisement : BaseEntity, ISoftDeletable
    {
        public Guid EmployerId { get; set; }

        public Employer Employer { get; set; } = null!;

        public Guid JobPositionId { get; set; }

        public JobPosition JobPosition { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Experience { get; set; }

        public string[] Skills { get; set; } = [];

        public string? City { get; set; }

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }

        public string? Currency { get; set; }

        public int OpenPositions { get; set; }

        public JobType JobType { get; set; }

        public DateOnly Deadline { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? DeletedAt { get; set; }

        public ICollection<JobApplication> JobApplications { get; set; } = [];
    }
}
