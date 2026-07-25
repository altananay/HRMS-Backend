namespace Domain.Entities
{
    /// <summary>Join entity between <see cref="User"/> and <see cref="Role"/>, keyed on both ids.</summary>
    public class UserRole
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public User User { get; set; } = null!;

        public Role Role { get; set; } = null!;
    }
}
