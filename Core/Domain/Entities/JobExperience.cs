using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// A past role on a CV.
    /// </summary>
    /// <remarks>
    /// A real entity rather than an owned type: it is independently editable and carries its own
    /// identity and timestamps. Note that CvManager previously regenerated a fresh ObjectId for
    /// every experience on <i>every</i> CV update, so child identity was never stable across edits.
    /// Here the key is assigned once, on creation.
    /// </remarks>
    public class JobExperience : BaseEntity
    {
        public Guid CvId { get; set; }

        public Cv Cv { get; set; } = null!;

        public string CompanyName { get; set; } = null!;

        public string? Department { get; set; }

        public string Position { get; set; } = null!;

        public int? StartYear { get; set; }

        /// <summary><c>null</c> means the seeker still works there.</summary>
        public int? EndYear { get; set; }

        public string? Description { get; set; }
    }
}
