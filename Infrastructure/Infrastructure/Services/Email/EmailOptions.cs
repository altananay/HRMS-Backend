using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Email
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";

        public string? Host { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; } = 1025;

        [EmailAddress]
        public string From { get; set; } = "no-reply@hrms.local";

        public string FromName { get; set; } = "HRMS";

        public bool UseStartTls { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
    }
}
