using System.Collections.Concurrent;
using System.Net;
using Infrastructure.Captcha;
using Infrastructure.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace API.Tests.Infrastructure;

/// <summary>Stands in for Cloudflare Turnstile: only <see cref="ValidToken"/> passes.</summary>
public sealed class FakeCaptchaVerifier : ICaptchaVerifier
{
    public const string ValidToken = "valid-captcha";

    public Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default) =>
        Task.FromResult(token == ValidToken);
}

/// <summary>Records e-mails instead of sending them; fails for messages whose body contains <see cref="FailMarker"/>.</summary>
public sealed class RecordingEmailSender : IEmailSender
{
    public const string FailMarker = "[smtp-down]";
    public ConcurrentBag<EmailMessage> Sent { get; } = [];

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (message.Body.Contains(FailMarker)) throw new IOException("SMTP server unavailable");
        Sent.Add(message);
        return Task.CompletedTask;
    }
}

/// <summary>
/// TestServer has no client IP; this lets each test pick one with the X-Test-Client-Ip header so
/// per-IP rate limiting can be exercised without tests sharing a quota.
/// </summary>
public sealed class TestClientIpStartupFilter : IStartupFilter
{
    public const string Header = "X-Test-Client-Ip";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use((context, nextMiddleware) =>
        {
            if (context.Request.Headers.TryGetValue(Header, out var ip) && IPAddress.TryParse(ip, out var address))
                context.Connection.RemoteIpAddress = address;
            return nextMiddleware(context);
        });
        next(app);
    };
}
