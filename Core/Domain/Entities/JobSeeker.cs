using Domain.Common;

namespace Domain.Entities
{
    public class JobSeeker : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? NationalityId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public Cv Cv { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool Status { get; set; }
        public string[] Claims { get; set; }
    }
}