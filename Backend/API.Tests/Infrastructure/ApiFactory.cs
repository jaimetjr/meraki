using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
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
        Environment.SetEnvironmentVariable("AzureBlobStorage__ConnectionString", "UseDevelopmentStorage=true");
        Environment.SetEnvironmentVariable("AzureBlobStorage__ContainerName", "test");

        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_db.GetConnectionString()).Options;
        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
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
