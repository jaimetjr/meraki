using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Captcha;
using Infrastructure.Data;
using Infrastructure.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(API.Tests.Infrastructure.ApiFactory))]

namespace API.Tests.Infrastructure;

/// <summary>
/// One API instance + one throwaway Postgres container for the whole test run.
/// Tests must create their own data (unique names) and never rely on seeded rows.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Password = "Test-Passw0rd!";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private readonly ConcurrentDictionary<UserRole, Task<string>> _tokens = new();

    /// <summary>Where uploaded images land during tests (local-disk storage provider).</summary>
    public string UploadsRoot { get; } = Path.Combine(Path.GetTempPath(), $"meraki-tests-{Guid.NewGuid():N}");
    public const string UploadsBaseUrl = "http://localhost/uploads";

    /// <summary>The SuperAdmin the app creates at startup from Seed:SuperAdmin configuration.</summary>
    public const string SeedSuperAdminEmail = "owner@test.local";
    public const string SeedSuperAdminPassword = "Owner-Passw0rd!";

    /// <summary>The frontend origin CORS is configured to allow in tests.</summary>
    public const string AllowedOrigin = "http://admin.test";

    public const string ClinicEmail = "clinic@test.local";
    public const int ContactRateLimitPerHour = 3;

    /// <summary>E-mails the app "sent" during the test run.</summary>
    public RecordingEmailSender Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // External services (Cloudflare, SMTP) are replaced at their boundary.
            services.RemoveAll<ICaptchaVerifier>();
            services.AddSingleton<ICaptchaVerifier, FakeCaptchaVerifier>();
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.AddSingleton<IStartupFilter, TestClientIpStartupFilter>();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _db.StartAsync();

        // Program.cs reads configuration eagerly while building the host, so values must be
        // present before the host is created; environment variables are the reliable channel.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "test-secret-key-that-is-long-enough-for-hmac-sha256!!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "MerakiAPI");
        Environment.SetEnvironmentVariable("Jwt__Audience", "MerakiAPI");
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        Environment.SetEnvironmentVariable("Storage__Provider", "Local");
        Environment.SetEnvironmentVariable("Storage__Local__RootPath", UploadsRoot);
        Environment.SetEnvironmentVariable("Storage__PublicBaseUrl", UploadsBaseUrl);
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", AllowedOrigin);
        Environment.SetEnvironmentVariable("Contact__NotifyEmail", ClinicEmail);
        Environment.SetEnvironmentVariable("Contact__RateLimitPerHour", ContactRateLimitPerHour.ToString());
        Environment.SetEnvironmentVariable("Seed__SuperAdmin__Email", SeedSuperAdminEmail);
        Environment.SetEnvironmentVariable("Seed__SuperAdmin__Password", SeedSuperAdminPassword);

        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_db.GetConnectionString()).Options;
        await using (var context = new AppDbContext(options))
        {
            await context.Database.MigrateAsync();
        }

        // Start the app (and its startup seeding) now, before any test touches the database.
        CreateClient().Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
        if (Directory.Exists(UploadsRoot)) Directory.Delete(UploadsRoot, recursive: true);
    }

    /// <summary>Maps a URL returned by the API to the file it points at in <see cref="UploadsRoot"/>.</summary>
    public string UploadedFilePath(string url) =>
        Path.Combine(UploadsRoot, url[(UploadsBaseUrl.Length + 1)..].Replace('/', Path.DirectorySeparatorChar));

    /// <summary>A brand-new, migrated database in the same container (for testing the seeder in isolation).</summary>
    public async Task<AppDbContext> CreateFreshDatabaseAsync()
    {
        var connection = new Npgsql.NpgsqlConnectionStringBuilder(_db.GetConnectionString()) { Database = $"fresh_{Guid.NewGuid():N}" };
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection.ConnectionString).Options);
        await context.Database.MigrateAsync();
        return context;
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_db.GetConnectionString()).Options;
        return new AppDbContext(options);
    }

    /// <summary>Inserts an active user directly in the database and returns its id and e-mail.</summary>
    public async Task<(Guid Id, string Email)> CreateUserAsync(UserRole role, string? password = null, bool active = true)
    {
        var email = $"{role.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}@test.local";
        var user = new User(Guid.NewGuid(), email, BCrypt.Net.BCrypt.HashPassword(password ?? Password), role);
        if (!active) user.Deactivate();

        await using var db = CreateDbContext();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var response = await CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    /// <summary>A client authenticated as a freshly created user of the given role (cached per role).</summary>
    public async Task<HttpClient> ClientAsAsync(UserRole role)
    {
        var token = await _tokens.GetOrAdd(role, async r =>
        {
            var (_, email) = await CreateUserAsync(r);
            return await LoginAsync(email, Password);
        });
        return ClientWithToken(token);
    }

    public HttpClient ClientWithToken(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
