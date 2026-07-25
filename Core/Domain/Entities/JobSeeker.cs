using Domain.Enums;

namespace Domain.Entities
{
    public class JobSeeker : User
    {
        public JobSeeker() : base(UserType.JobSeeker) { }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        /// <summary>Turkish national ID (TCKN). PII — never logged, never returned in a DTO.</summary>
        public string? NationalId { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        /// <summary>
        /// One CV per seeker, by a unique foreign key on <c>cvs.job_seeker_id</c>.
        /// </summary>
        /// <remarks>
        /// Previously the CV was written twice — once into the <c>cvs</c> collection and once
        /// embedded whole inside the JobSeeker document — by CvManager calling
        /// <c>_jobSeekerService.UpdateCvById(...)</c> after every insert and update. The two copies
        /// could and did drift apart. Now there is one row and one owner.
        /// </remarks>
        public Cv? Cv { get; set; }

        public ICollection<JobApplication> JobApplications { get; set; } = [];
    }
}
