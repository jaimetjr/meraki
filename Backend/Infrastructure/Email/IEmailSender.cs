namespace Infrastructure.Email;

public sealed record EmailMessage(string To, string Subject, string Body, string? ReplyTo = null);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
