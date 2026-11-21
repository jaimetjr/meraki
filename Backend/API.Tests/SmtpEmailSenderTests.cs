using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Infrastructure.Email;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests;

/// <summary>Runs Mailpit, a throwaway SMTP server whose HTTP API shows what it received.</summary>
public sealed class MailpitFixture : IAsyncLifetime
{
    private readonly IContainer _mailpit = new ContainerBuilder()
        .WithImage("axllent/mailpit:v1.21")
        .WithPortBinding(1025, assignRandomHostPort: true)
        .WithPortBinding(8025, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/api/v1/info").ForPort(8025)))
        .Build();

    public string Host => _mailpit.Hostname;
    public int SmtpPort => _mailpit.GetMappedPublicPort(1025);
    public HttpClient Api() => new() { BaseAddress = new Uri($"http://{_mailpit.Hostname}:{_mailpit.GetMappedPublicPort(8025)}") };

    public ValueTask InitializeAsync() => new(_mailpit.StartAsync());
    public ValueTask DisposeAsync() => _mailpit.DisposeAsync();
}

public class SmtpEmailSenderTests(MailpitFixture mailpit) : IClassFixture<MailpitFixture>
{
    [Fact]
    public async Task Gives_up_within_its_timeout_when_the_server_accepts_but_never_answers()
    {
        // A server that accepts the TCP connection and then says nothing (not even the SMTP greeting).
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync(TestContext.Current.CancellationToken);
        var sender = new SmtpEmailSender(
            new SmtpOptions { Host = "127.0.0.1", Port = port, From = "site@meraki.test", TimeoutSeconds = 1 },
            NullLogger<SmtpEmailSender>.Instance);

        var send = sender.SendAsync(new EmailMessage("clinic@meraki.test", "Assunto", "Corpo"), TestContext.Current.CancellationToken);
        var finished = await Task.WhenAny(send, Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        (await accepted).Dispose(); // lets a still-hanging send fail fast after the assertion
        Assert.Same(send, finished);
        await Assert.ThrowsAnyAsync<Exception>(() => send);
    }

    [Fact]
    public async Task Delivers_the_message_with_reply_to_over_smtp()
    {
        var sender = new SmtpEmailSender(
            new SmtpOptions { Host = mailpit.Host, Port = mailpit.SmtpPort, From = "site@meraki.test" },
            NullLogger<SmtpEmailSender>.Instance);
        var subject = $"Novo contato {Guid.NewGuid():N}";

        await sender.SendAsync(new EmailMessage("clinic@meraki.test", subject, "Olá, quero agendar.", ReplyTo: "visitor@example.com"));

        using var api = mailpit.Api();
        var list = await api.GetFromJsonAsync<JsonElement>("/api/v1/messages");
        var summary = list.GetProperty("messages").EnumerateArray().Single(m => m.GetProperty("Subject").GetString() == subject);
        var message = await api.GetFromJsonAsync<JsonElement>($"/api/v1/message/{summary.GetProperty("ID").GetString()}");
        Assert.Equal("site@meraki.test", message.GetProperty("From").GetProperty("Address").GetString());
        Assert.Equal("clinic@meraki.test", message.GetProperty("To")[0].GetProperty("Address").GetString());
        Assert.Equal("visitor@example.com", message.GetProperty("ReplyTo")[0].GetProperty("Address").GetString());
        Assert.Contains("Olá, quero agendar.", message.GetProperty("Text").GetString());
    }
}
