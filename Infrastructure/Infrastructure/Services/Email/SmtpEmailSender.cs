using Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly string _host;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;

            _host = _options.IsConfigured
                ? _options.Host!
                : throw new InvalidOperationException(
                    "SmtpEmailSender was registered without Email:Host. Configure a host or let " +
                    "AddEmail fall back to LoggingEmailSender.");
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress(_options.FromName, _options.From));
            mail.To.Add(MailboxAddress.Parse(message.To));
            mail.Subject = message.Subject;
            mail.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

            using var client = new SmtpClient();

            var security = _options.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_host, _options.Port, security, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                await client.AuthenticateAsync(_options.UserName, _options.Password ?? string.Empty, cancellationToken);
            }

            await client.SendAsync(mail, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);

            _logger.LogInformation("Sent mail {Subject} to {Recipient}", message.Subject, message.To);
        }
    }
}
