using Domain.Common;

namespace Domain.Entities
{
    /// <summary>A message from the public contact form. Standalone — no relationships.</summary>
    public class Contact : BaseEntity
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string Message { get; set; } = null!;

        public bool IsHandled { get; set; }
    }
}
