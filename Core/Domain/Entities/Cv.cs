using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities
{
    /// <summary>
    /// A job seeker's CV. One per seeker, enforced by a unique index on <see cref="JobSeekerId"/>.
    /// </summary>
    /// <remarks>
    /// The seeker's own details — first name, last name, national ID, date of birth, email — used
    /// to be copied onto every CV document by CvManager. Those copies are gone; response DTOs
    /// project them from the <see cref="JobSeeker"/> navigation instead, so the JSON the client sees
    /// is unchanged while the database keeps a single source of truth.
    /// </remarks>
    public class Cv : BaseEntity
    {
        public Guid JobSeekerId { get; set; }

        public JobSeeker JobSeeker { get; set; } = null!;

        public string? Information { get; set; }

        public string? ImageUrl { get; set; }

        /// <summary>Free text. The unused <c>Hobby</c> value object that never mapped to it is deleted.</summary>
        public string? Hobbies { get; set; }

        /// <summary>PostgreSQL <c>text[]</c> with a GIN index, so "find CVs with skill X" is indexable.</summary>
        public string[] Skills { get; set; } = [];

        /// <summary>Owned type — three inline nullable columns, not a separate table.</summary>
        public SocialMedia? SocialMedia { get; set; }

        public ICollection<Education> Educations { get; set; } = [];

        public ICollection<JobExperience> JobExperiences { get; set; } = [];

        public ICollection<CvLanguage> Languages { get; set; } = [];

        public ICollection<CvProject> Projects { get; set; } = [];

        public ICollection<CvFile> Files { get; set; } = [];
    }
}
