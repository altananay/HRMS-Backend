namespace Application.Abstractions
{
    public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);

    /// <summary>
    /// Sends transactional mail.
    /// </summary>
    /// <remarks>
    /// The only caller today is the password-reset flow. Deliberately an abstraction rather than a
    /// direct SMTP call, because the delivery mechanism is the part most likely to change and the
    /// part tests must not exercise: the unit suite substitutes it, and a deployment with no SMTP
    /// configured falls back to writing the message to the log instead of failing to start.
    /// </remarks>
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
