using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// A seeker's application to an advertisement.
    /// </summary>
    /// <remarks>
    /// <c>EmployerId</c> is deliberately absent. It was stored here as a copy of the advertisement's
    /// employer, which is derivable via <c>JobAdvertisement.EmployerId</c> and could therefore go
    /// stale. Querying an employer's applications is now a join across an indexed foreign key.
    ///
    /// A unique index on (<see cref="JobSeekerId"/>, <see cref="JobAdvertisementId"/>) stops a
    /// seeker applying to the same advertisement repeatedly — nothing prevented that before.
    /// </remarks>
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
