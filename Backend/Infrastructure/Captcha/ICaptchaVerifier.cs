namespace Infrastructure.Captcha;

/// <summary>Checks a CAPTCHA token produced by the visitor's browser.</summary>
public interface ICaptchaVerifier
{
    Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default);
}
