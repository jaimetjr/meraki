using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Captcha;

public sealed class TurnstileOptions
{
    /// <summary>Turnstile secret key (Cloudflare dashboard). Dev/test can use Cloudflare's documented test secrets.</summary>
    public string SecretKey { get; set; } = string.Empty;
}

/// <summary>
/// Verifies Cloudflare Turnstile tokens with the siteverify endpoint. Fails closed: any error or
/// missing configuration rejects the token.
/// </summary>
public sealed class TurnstileVerifier(HttpClient http, TurnstileOptions options, ILogger<TurnstileVerifier> logger) : ICaptchaVerifier
{
    private const string SiteVerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            logger.LogError("Turnstile:SecretKey is not configured; rejecting CAPTCHA tokens.");
            return false;
        }

        var form = new Dictionary<string, string> { ["secret"] = options.SecretKey, ["response"] = token };
        if (!string.IsNullOrWhiteSpace(remoteIp)) form["remoteip"] = remoteIp;

        try
        {
            using var response = await http.PostAsync(SiteVerifyUrl, new FormUrlEncodedContent(form), cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<SiteVerifyResponse>(cancellationToken);
            if (result is { Success: true }) return true;

            logger.LogInformation("Turnstile rejected a token: {Errors}", string.Join(", ", result?.ErrorCodes ?? []));
            return false;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(exception, "Could not reach Turnstile; rejecting the token.");
            return false;
        }
    }

    private sealed record SiteVerifyResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("error-codes")] string[]? ErrorCodes);
}
