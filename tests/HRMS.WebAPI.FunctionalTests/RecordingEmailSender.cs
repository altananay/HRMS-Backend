using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Application.Abstractions;

namespace HRMS.WebAPI.FunctionalTests;

public sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();

    public IReadOnlyCollection<EmailMessage> Sent => _sent;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _sent.Enqueue(message);
        return Task.CompletedTask;
    }

    public void Clear() => _sent.Clear();

    public EmailMessage? LastTo(string recipient)
        => _sent.LastOrDefault(message =>
            string.Equals(message.To, recipient, StringComparison.OrdinalIgnoreCase));

    public string? ResetTokenFor(string recipient)
    {
        var body = LastTo(recipient)?.TextBody;
        if (body is null)
        {
            return null;
        }

        var match = Regex.Match(body, @"[?&]token=([^\s&]+)");

        return match.Success ? Uri.UnescapeDataString(match.Groups[1].Value) : null;
    }
}
