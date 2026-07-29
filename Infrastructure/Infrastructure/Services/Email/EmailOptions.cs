using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Email
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";

        /// <summary>
        /// Leave empty to disable SMTP entirely; mail is then written to the log instead.
        /// </summary>
        /// <remarks>
        /// Not a required field on purpose. A missing mail server must not stop the application from
        /// starting — it degrades the reset flow, it does not break the product. In development this
        /// points at the Mailpit container on localhost:1025.
        /// </remarks>
        public string? Host { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; } = 1025;

        [EmailAddress]
        public string From { get; set; } = "no-reply@hrms.local";

        public string FromName { get; set; } = "HRMS";

        /// <summary>Off for Mailpit, which speaks plain SMTP on the loopback interface.</summary>
        public bool UseStartTls { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
    }
}
