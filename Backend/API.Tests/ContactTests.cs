using System.Net;
using System.Net.Http.Json;
using API.Tests.Infrastructure;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

/// <summary>The public contact form: consent + CAPTCHA, stored for the admin inbox, e-mail notification.</summary>
public class ContactTests(ApiFactory api)
{
    private static int _nextIp;

    /// <summary>A client with its own IP so per-IP rate limiting never leaks between tests.</summary>
    private HttpClient Visitor()
    {
        var client = api.CreateClient();
        var n = Interlocked.Increment(ref _nextIp);
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.Header, $"10.0.{n / 250}.{n % 250 + 1}");
        return client;
    }

    private static object Message(string name, bool consent = true, string token = FakeCaptchaVerifier.ValidToken,
        string email = "visitor@example.com", string? message = "Gostaria de agendar uma sessão.") => new
    {
        name, email, phone = "(11) 99999-0000", service = "Terapia", message, consent, captchaToken = token,
    };

    private async Task<List<System.Text.Json.JsonElement>> InboxAsync() =>
        (await (await api.ClientAsAsync(UserRole.Admin)).GetJson("/api/contact")).EnumerateArray().ToList();

    [Fact]
    public async Task A_valid_message_is_stored_unread_and_the_clinic_is_notified()
    {
        var name = Http.Unique("Visitor");

        var response = await Visitor().PostAsJsonAsync("/api/contact", Message(name));

        Assert.True((await response.Json()).GetProperty("success").GetBoolean());
        var stored = (await InboxAsync()).Single(m => m.Str("name") == name);
        Assert.False(stored.GetProperty("isRead").GetBoolean());
        Assert.Equal("Gostaria de agendar uma sessão.", stored.Str("message"));
        var email = Assert.Single(api.Emails.Sent, e => e.Body.Contains(name));
        Assert.Equal(ApiFactory.ClinicEmail, email.To);
        Assert.Equal("visitor@example.com", email.ReplyTo);
        Assert.Contains("Gostaria de agendar uma sessão.", email.Body);
    }

    [Fact]
    public async Task Without_consent_the_message_is_rejected_and_not_stored()
    {
        var name = Http.Unique("NoConsent");

        var response = await Visitor().PostAsJsonAsync("/api/contact", Message(name, consent: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(await InboxAsync(), m => m.Str("name") == name);
    }

    [Fact]
    public async Task A_failed_captcha_is_rejected_and_not_stored()
    {
        var name = Http.Unique("Bot");

        var response = await Visitor().PostAsJsonAsync("/api/contact", Message(name, token: "forged"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(await InboxAsync(), m => m.Str("name") == name);
    }

    [Fact]
    public async Task An_invalid_email_address_is_rejected()
    {
        var response = await Visitor().PostAsJsonAsync("/api/contact", Message(Http.Unique("V"), email: "not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_message_is_kept_when_the_notification_email_fails()
    {
        var name = Http.Unique("Kept");

        var response = await Visitor().PostAsJsonAsync("/api/contact", Message(name, message: $"oi {RecordingEmailSender.FailMarker}"));

        Assert.True((await response.Json()).GetProperty("success").GetBoolean());
        Assert.Contains(await InboxAsync(), m => m.Str("name") == name);
    }

    [Fact]
    public async Task Admin_can_mark_a_message_read_and_unread()
    {
        var name = Http.Unique("Read");
        await (await Visitor().PostAsJsonAsync("/api/contact", Message(name))).Json();
        var id = (await InboxAsync()).Single(m => m.Str("name") == name).Id();
        var admin = await api.ClientAsAsync(UserRole.Admin);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PatchAsJsonAsync($"/api/contact/{id}/read", new { isRead = true })).StatusCode);
        Assert.True((await InboxAsync()).Single(m => m.Id() == id).GetProperty("isRead").GetBoolean());

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PatchAsJsonAsync($"/api/contact/{id}/read", new { isRead = false })).StatusCode);
        Assert.False((await InboxAsync()).Single(m => m.Id() == id).GetProperty("isRead").GetBoolean());
    }

    [Fact]
    public async Task Deleting_a_message_removes_it_permanently()
    {
        var name = Http.Unique("Erase");
        await (await Visitor().PostAsJsonAsync("/api/contact", Message(name))).Json();
        var id = (await InboxAsync()).Single(m => m.Str("name") == name).Id();

        var response = await (await api.ClientAsAsync(UserRole.Admin)).DeleteAsync($"/api/contact/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var db = api.CreateDbContext();
        Assert.False(await db.ContactMessages.IgnoreQueryFilters().AnyAsync(m => m.Id == id));
    }

    [Fact]
    public async Task The_audit_log_never_copies_the_message_contents()
    {
        var name = Http.Unique("Private");
        await (await Visitor().PostAsJsonAsync("/api/contact", Message(name))).Json();
        var id = (await InboxAsync()).Single(m => m.Str("name") == name).Id();

        await (await api.ClientAsAsync(UserRole.Admin)).DeleteAsync($"/api/contact/{id}");

        await using var db = api.CreateDbContext();
        var logs = await db.AuditLogs.Where(a => a.EntityId == id).ToListAsync();
        Assert.Equal(["Create", "Delete"], logs.OrderBy(l => l.Timestamp).Select(l => l.Action));
        Assert.All(logs, l => Assert.True(l.OldValues is null && l.NewValues is null));
    }

    [Theory]
    [InlineData("GET", "/api/contact")]
    [InlineData("PATCH", "/api/contact/00000000-0000-0000-0000-000000000001/read")]
    [InlineData("DELETE", "/api/contact/00000000-0000-0000-0000-000000000001")]
    public async Task Anonymous_visitors_cannot_read_or_manage_messages(string method, string url)
    {
        var response = await api.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Too_many_messages_from_one_address_are_rate_limited()
    {
        var visitor = Visitor();
        for (var i = 0; i < ApiFactory.ContactRateLimitPerHour; i++)
            Assert.Equal(HttpStatusCode.OK, (await visitor.PostAsJsonAsync("/api/contact", Message(Http.Unique("Flood")))).StatusCode);

        var response = await visitor.PostAsJsonAsync("/api/contact", Message(Http.Unique("Flood")));

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task Behind_a_proxy_each_visitor_still_gets_their_own_quota()
    {
        // On Railway every request comes from its proxy; the visitor's own address is in X-Forwarded-For.
        HttpClient ThroughProxy(string visitorIp)
        {
            var client = api.CreateClient();
            client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.Header, "172.16.0.1");
            client.DefaultRequestHeaders.Add("X-Forwarded-For", visitorIp);
            return client;
        }
        var flooder = ThroughProxy("10.1.0.1");
        for (var i = 0; i < ApiFactory.ContactRateLimitPerHour; i++)
            await flooder.PostAsJsonAsync("/api/contact", Message(Http.Unique("Flood")));
        Assert.Equal(HttpStatusCode.TooManyRequests,
            (await flooder.PostAsJsonAsync("/api/contact", Message(Http.Unique("Flood")))).StatusCode);

        var response = await ThroughProxy("10.1.0.2").PostAsJsonAsync("/api/contact", Message(Http.Unique("Other")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
