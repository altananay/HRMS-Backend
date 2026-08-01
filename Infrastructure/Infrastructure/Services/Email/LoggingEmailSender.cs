using Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Email
{
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
