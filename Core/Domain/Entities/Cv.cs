using Domain.Common;

namespace Domain.Entities
{
    public class Cv : BaseEntity
    {
        public string JobSeekerId { get; set; }
        public Education[] Educations { get; set; }
        public JobExperience[] Experiences { get; set; }
        public Language[] Languages { get; set; }
        public string ImageUrl { get; set; }
        public SocialMedia SocialMedia { get; set; }
        public string Skills { get; set; }
        public string Information { get; set; }
    }
}