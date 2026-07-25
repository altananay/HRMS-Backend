using Domain.Enums;

namespace Domain.Entities
{
    public class SystemStaff : User
    {
        public SystemStaff() : base(UserType.SystemStaff) { }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;
    }
}
