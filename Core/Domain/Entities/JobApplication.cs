using Domain.Common;

namespace Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public string JobAdvertisementId { get; set; }   
        public string JobSeekerId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}