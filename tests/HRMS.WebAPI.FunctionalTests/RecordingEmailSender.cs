using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Application.Abstractions;

namespace HRMS.WebAPI.FunctionalTests;

/// <summary>
/// Captures outgoing mail instead of sending it, and pulls the reset token back out of the body.
/// </summary>
/// <remarks>
/// Asserting on the body rather than reading the token from the database is deliberate: the link is
/// the part the frontend consumes, so a malformed one — wrong base URL, unescaped token, missing
/// query parameter — is a real defect that a database-level assertion would never see.
/// </remarks>
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

    /// <summary>The <c>token</c> query parameter of the reset link in the last mail to this address.</summary>
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
