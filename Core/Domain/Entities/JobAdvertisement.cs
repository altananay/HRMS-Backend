using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// A published job advertisement.
    /// </summary>
    /// <remarks>
    /// Five denormalized columns are gone: <c>CompanyName</c>, <c>CompanyPhone</c>, <c>WebSite</c>
    /// and <c>Email</c> were stamped onto every advertisement from the employer at insert time and
    /// then never refreshed, and <c>JobPosition</c> duplicated the position's name alongside its id.
    /// Response DTOs project all five through the <see cref="Employer"/> and
    /// <see cref="JobPosition"/> navigations, so the JSON contract is unchanged.
    ///
    /// Soft-deletable: an employer removing an advertisement must not erase the applications
    /// attached to it.
    /// </remarks>
    public class JobAdvertisement : BaseEntity, ISoftDeletable
    {
        public Guid EmployerId { get; set; }

        public Employer Employer { get; set; } = null!;

        public Guid JobPositionId { get; set; }

        public JobPosition JobPosition { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Experience { get; set; }

        /// <summary>PostgreSQL <c>text[]</c> with a GIN index for skill search.</summary>
        public string[] Skills { get; set; } = [];

        public string? City { get; set; }

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }

        public string? Currency { get; set; }

        public int OpenPositions { get; set; }

        public JobType JobType { get; set; }

        public DateOnly Deadline { get; set; }

        /// <summary>
        /// Whether the advertisement is live.
        /// </summary>
        /// <remarks>
        /// Previously <c>Status</c>, and <c>JobAdvertisementManager.Update</c> never copied it onto
        /// the replacement document — so editing any advertisement silently deactivated it. Change
        /// tracking makes that impossible now: an untouched property is not written at all.
        /// </remarks>
        public bool IsActive { get; set; } = true;

        public DateTime? DeletedAt { get; set; }

        public ICollection<JobApplication> JobApplications { get; set; } = [];
    }
}
