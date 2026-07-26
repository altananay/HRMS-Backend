using Domain.Enums;

namespace Domain.Entities
{
    public class JobSeeker : User
    {
        public JobSeeker() : base(UserType.JobSeeker) { }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? NationalId { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public Cv? Cv { get; set; }

        public ICollection<JobApplication> JobApplications { get; set; } = [];
    }
}
