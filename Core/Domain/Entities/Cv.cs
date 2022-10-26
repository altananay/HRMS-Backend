using Domain.Common;

namespace Domain.Entities
{
    public class Cv : BaseEntity
    {
        public string JobSeekerId { get; set; }
        public Education[] Educations { get; set; }
        public JobExperience[]? JobExperiences { get; set; }
        public bool JobExperience { get; set; }
        public ProgrammingLanguage[]? ProgrammingLanguages { get; set; }
        public bool ProgrammingLanguage { get; set; }
        public LibraryAndFramework[]? LibraryAndFrameworks { get; set; }
        public bool LibraryAndFramework { get; set; }
        public Skill[] Skills { get; set; }
        public Language[]? Languages { get; set; }
        public bool Language { get; set; }
        public Project[]? Projects { get; set; }
        public bool Project { get; set; }
        public string? ImageUrl { get; set; }
        public SocialMedia? SocialMedias { get; set; }
        public bool SocialMedia { get; set; }
        public string Information { get; set; }
        public Hobby? Hobbies { get; set; }
        public bool Hobby { get; set; }
    }
}