using Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Email
{
    /// <summary>
    /// The fallback when no SMTP host is configured: writes the message to the log instead of sending.
    /// </summary>
    /// <remarks>
    /// Registered only when <c>Email:Host</c> is empty. It exists so a fresh clone can exercise the
    /// reset flow without any mail infrastructure — the developer reads the link out of the console
    /// or Seq.
    ///
    /// The body is written **only outside Production**. A reset link is a bearer credential for the
    /// next hour, and a production log is the wrong place for one; there the entry records that a
    /// mail was due and nothing more.
    /// </remarks>
    public sealed class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;
        private readonly IHostEnvironment _environment;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (_environment.IsProduction())
            {
                _logger.LogWarning(
                    "No SMTP host is configured, so {Subject} for {Recipient} was not delivered.",
                    message.Subject, message.To);
            }
            else
            {
                _logger.LogInformation(
                    "No SMTP host configured. Mail {Subject} for {Recipient} would have said:\n{Body}",
                    message.Subject, message.To, message.TextBody);
            }

            return Task.CompletedTask;
        }
    }
}
