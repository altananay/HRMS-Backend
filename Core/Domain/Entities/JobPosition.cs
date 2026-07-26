using Domain.Common;

namespace Domain.Entities
{
    public class JobPosition : BaseEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<JobAdvertisement> JobAdvertisements { get; set; } = [];
    }
}
