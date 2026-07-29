using Domain.Enums;

namespace Application.Common.Models
{
    /// <summary>
    /// The optional narrowing applied to a job application listing.
    /// </summary>
    /// <remarks>
    /// Three of these are <c>Guid?</c>. As positional parameters two of them could be swapped without
    /// the compiler noticing, and the result — an employer seeing another employer's applications —
    /// would look like data rather than a bug. Named members remove that class of mistake.
    ///
    /// <c>EmployerId</c> and <c>JobSeekerId</c> are overridden by the controller from the caller's
    /// token; only an admin's values survive as supplied.
    /// </remarks>
    public sealed record JobApplicationFilter
    {
        public Guid? EmployerId { get; init; }

        public Guid? JobSeekerId { get; init; }

        /// <summary>Applications received by one advertisement — the employer's per-listing view.</summary>
        public Guid? JobAdvertisementId { get; init; }

        public JobApplicationStatus? Status { get; init; }

        public static JobApplicationFilter None { get; } = new();
    }
}
