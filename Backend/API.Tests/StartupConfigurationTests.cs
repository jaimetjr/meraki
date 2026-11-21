using API.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace API.Tests;

/// <summary>
/// Loads the committed appsettings files exactly as the API does, so a production deploy that forgets a
/// setting fails at startup instead of saving image URLs that point at localhost or failing at send time.
/// </summary>
public class StartupConfigurationTests
{
    private sealed class Env(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "API";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static IConfiguration Settings(string environment, params (string Key, string Value)[] overrides) =>
        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.{environment}.json"), optional: true)
            .AddInMemoryCollection(overrides.Select(o => new KeyValuePair<string, string?>(o.Key, o.Value)))
            .Build();

    private static readonly (string, string)[] R2Credentials =
    [
        ("Storage:Provider", "R2"), ("Storage:R2:AccountId", "acc"), ("Storage:R2:AccessKeyId", "key"),
        ("Storage:R2:SecretAccessKey", "secret"), ("Storage:R2:Bucket", "meraki"),
    ];

    [Fact]
    public void Production_with_R2_but_no_public_url_refuses_to_start()
    {
        var settings = Settings("Production", R2Credentials);

        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileStorage(settings, new Env("Production")));
        Assert.Contains("Storage:PublicBaseUrl", error.Message);
    }

    [Fact]
    public void Production_with_local_storage_but_no_public_url_refuses_to_start()
    {
        var settings = Settings("Production");

        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddFileStorage(settings, new Env("Production")));
        Assert.Contains("Storage:PublicBaseUrl", error.Message);
    }

    [Fact]
    public void Production_with_smtp_but_no_host_refuses_to_start()
    {
        var settings = Settings("Production", ("Email:Smtp:From", "site@clinic.com.br"));

        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddContactForm(settings));
        Assert.Contains("Email:Smtp:Host", error.Message);
    }

    [Fact]
    public void Production_with_smtp_but_no_sender_address_refuses_to_start()
    {
        var settings = Settings("Production", ("Email:Smtp:Host", "smtp.clinic.com.br"));

        var error = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddContactForm(settings));
        Assert.Contains("Email:Smtp:From", error.Message);
    }

    [Fact]
    public void Development_starts_with_only_the_committed_settings()
    {
        var settings = Settings("Development");

        new ServiceCollection().AddFileStorage(settings, new Env("Development"));
        new ServiceCollection().AddContactForm(settings);
    }
}
