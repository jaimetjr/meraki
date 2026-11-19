using System.Net;
using System.Text;
using Infrastructure.Captcha;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests;

/// <summary>TurnstileVerifier against a scripted Cloudflare siteverify endpoint.</summary>
public class TurnstileVerifierTests
{
    private sealed class ScriptedHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(HttpRequestMessage Request, Dictionary<string, string> Form)> Calls { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            var form = body.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('='))
                .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => Uri.UnescapeDataString(p[1].Replace('+', ' ')));
            Calls.Add((request, form));
            return respond(request);
        }
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static TurnstileVerifier Verifier(ScriptedHandler handler) =>
        new(new HttpClient(handler), new TurnstileOptions { SecretKey = "the-secret" }, NullLogger<TurnstileVerifier>.Instance);

    [Fact]
    public async Task Sends_secret_token_and_ip_to_siteverify_and_accepts_a_success()
    {
        var handler = new ScriptedHandler(_ => Json("""{"success":true}"""));

        var ok = await Verifier(handler).VerifyAsync("token-123", "203.0.113.9");

        Assert.True(ok);
        var (request, form) = Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://challenges.cloudflare.com/turnstile/v0/siteverify", request.RequestUri!.ToString());
        Assert.Equal("the-secret", form["secret"]);
        Assert.Equal("token-123", form["response"]);
        Assert.Equal("203.0.113.9", form["remoteip"]);
    }

    [Fact]
    public async Task Rejects_when_cloudflare_says_the_token_is_invalid()
    {
        var handler = new ScriptedHandler(_ => Json("""{"success":false,"error-codes":["invalid-input-response"]}"""));

        Assert.False(await Verifier(handler).VerifyAsync("token-123", null));
    }

    [Fact]
    public async Task Rejects_an_empty_token_without_calling_cloudflare()
    {
        var handler = new ScriptedHandler(_ => Json("""{"success":true}"""));

        Assert.False(await Verifier(handler).VerifyAsync("", null));
        Assert.Empty(handler.Calls);
    }

    [Fact]
    public async Task Fails_closed_when_cloudflare_is_unreachable()
    {
        var handler = new ScriptedHandler(_ => throw new HttpRequestException("network down"));

        Assert.False(await Verifier(handler).VerifyAsync("token-123", null));
    }
}
