using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Email;

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;

    /// <summary>587 (STARTTLS) by default; 465 uses implicit TLS.</summary>
    public int Port { get; set; } = 587;

    /// <summary>Leave empty for servers that accept unauthenticated mail (e.g. a local test server).</summary>
    public string? User { get; set; }
    public string? Password { get; set; }

    /// <summary>The sender address, e.g. "site@clinic.com.br" (must be allowed by the SMTP provider).</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Upper bound for one whole send (connect, log in, send). The contact form waits for it, so a slow or
    /// unreachable server must not hold the visitor's request for MailKit's default of minutes per step.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;
}

/// <summary>Sends e-mail through any SMTP server (clinic mailbox, Resend, SendGrid, Brevo…).</summary>
public sealed class SmtpEmailSender(SmtpOptions options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(options.From));
        mime.To.Add(MailboxAddress.Parse(message.To));
        if (!string.IsNullOrWhiteSpace(message.ReplyTo)) mime.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.Body };

        var limit = TimeSpan.FromSeconds(options.TimeoutSeconds);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(limit);
        using var client = new SmtpClient { Timeout = (int)limit.TotalMilliseconds };
        // Auto: implicit TLS on 465, STARTTLS when the server offers it otherwise.
        await client.ConnectAsync(options.Host, options.Port, SecureSocketOptions.Auto, timeout.Token);
        if (!string.IsNullOrWhiteSpace(options.User))
            await client.AuthenticateAsync(options.User, options.Password ?? string.Empty, timeout.Token);
        await client.SendAsync(FormatOptions.Default, new SendRequest(mime), timeout.Token);
        await client.DisconnectAsync(quit: true, timeout.Token);

        logger.LogInformation("Sent e-mail \"{Subject}\" to {To}", message.Subject, message.To);
    }
}

/// <summary>Development sender: writes the e-mail to the log instead of sending it.</summary>
public sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("E-mail (not sent, Email:Mode=Log)\nTo: {To}\nReply-To: {ReplyTo}\nSubject: {Subject}\n\n{Body}",
            message.To, message.ReplyTo, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}
