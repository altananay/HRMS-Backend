using Domain.Common;

namespace Domain.Entities
{
    public class JobAdvertisement : BaseEntity
    {
        public string EmployerId { get; set; }
        public string JobPositionId { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public int OpenPosition { get; set; }
        public string JobType { get; set; }
        public string JobTime { get; set; }
        public DateTime Deadline { get; set; }
        public bool Status { get; set; }
    }
}