namespace Application.Abstractions
{
    public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);

    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
