using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public Guid JobAdvertisementId { get; set; }

        public JobAdvertisement JobAdvertisement { get; set; } = null!;

        public Guid JobSeekerId { get; set; }

        public JobSeeker JobSeeker { get; set; } = null!;

        public string? JobSeekerNote { get; set; }

        public string? EmployerNote { get; set; }

        public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Submitted;

        public DateTime? StatusChangedAt { get; set; }
    }
}
