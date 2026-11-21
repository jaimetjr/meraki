using System.Threading.RateLimiting;
using API.Features.Contact;
using Infrastructure.Captcha;
using Infrastructure.Email;

namespace API.Common;

/// <summary>
/// Contact-form plumbing: Cloudflare Turnstile, e-mail (Email:Mode = Smtp | Log) and the per-IP rate
/// limit (Contact:RateLimitPerHour, default 10). Behind a reverse proxy, configure forwarded headers
/// or every visitor shares the proxy's IP (and its quota).
/// </summary>
public static class ContactSetup
{
    public static IServiceCollection AddContactForm(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ContactService>();

        services.AddSingleton(configuration.GetSection("Turnstile").Get<TurnstileOptions>() ?? new TurnstileOptions());
        services.AddHttpClient<ICaptchaVerifier, TurnstileVerifier>();

        switch ((configuration["Email:Mode"] ?? "Log").ToLowerInvariant())
        {
            case "smtp":
                var smtp = configuration.GetSection("Email:Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
                if (string.IsNullOrWhiteSpace(smtp.Host))
                    throw new InvalidOperationException("Email:Smtp:Host is required when Email:Mode is Smtp (or set Email:Mode to Log).");
                if (string.IsNullOrWhiteSpace(smtp.From))
                    throw new InvalidOperationException("Email:Smtp:From is required when Email:Mode is Smtp (the sender address).");
                services.AddSingleton(smtp);
                services.AddSingleton<IEmailSender, SmtpEmailSender>();
                break;
            case "log":
                services.AddSingleton<IEmailSender, LogEmailSender>();
                break;
            default:
                throw new InvalidOperationException($"Unknown Email:Mode '{configuration["Email:Mode"]}' (use Smtp or Log).");
        }

        var perHour = configuration.GetValue("Contact:RateLimitPerHour", 10);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(ContactController.RateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = perHour, Window = TimeSpan.FromHours(1), QueueLimit = 0 }));
        });

        return services;
    }
}
