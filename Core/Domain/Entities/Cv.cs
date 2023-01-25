using Domain.Common;

namespace Domain.Entities
{
    public class Cv : BaseEntity
    {
        public string JobSeekerId { get; set; }
        public Education[] Educations { get; set; }
        public JobExperience[]? JobExperiences { get; set; }
        public ProgrammingLanguage[]? ProgrammingLanguages { get; set; }
        public string? Skills { get; set; }
        public Language[]? Languages { get; set; }
        public Project[]? Projects { get; set; }
        public string? ImageUrl { get; set; }
        public SocialMedia? SocialMedias { get; set; }
        public string Information { get; set; }
        public string? Hobbies { get; set; }
    }
}