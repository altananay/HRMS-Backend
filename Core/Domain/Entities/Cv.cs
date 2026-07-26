using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Cv : BaseEntity
    {
        public Guid JobSeekerId { get; set; }

        public JobSeeker JobSeeker { get; set; } = null!;

        public string? Information { get; set; }

        public string? ImageUrl { get; set; }

        public string? Hobbies { get; set; }

        public string[] Skills { get; set; } = [];

        public SocialMedia SocialMedia { get; set; } = new();

        public ICollection<Education> Educations { get; set; } = [];

        public ICollection<JobExperience> JobExperiences { get; set; } = [];

        public ICollection<CvLanguage> Languages { get; set; } = [];

        public ICollection<CvProject> Projects { get; set; } = [];

        public ICollection<CvFile> Files { get; set; } = [];
    }
}
