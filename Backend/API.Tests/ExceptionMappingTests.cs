using System.Net;
using System.Net.Http.Headers;
using API.Tests.Infrastructure;
using Domain.Enums;
using Infrastructure.Storage;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Tests;

/// <summary>Only failed logins are 401; server-side permission errors must not look like an expired session.</summary>
public class ExceptionMappingTests(ApiFactory api)
{
    private sealed class StorageWithoutPermission : IFileStorage
    {
        public Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default) =>
            throw new UnauthorizedAccessException("Access to the path 'wwwroot/uploads' is denied.");

        public Task DeleteAsync(string url, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task A_storage_permission_error_is_a_500_not_a_401()
    {
        await using var app = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFileStorage>();
            services.AddSingleton<IFileStorage, StorageWithoutPermission>();
        }));
        var (_, email) = await api.CreateUserAsync(UserRole.Admin);
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await api.LoginAsync(email, ApiFactory.Password));
        var form = Http.Form(("Name", Http.Unique("T")), ("Bio", "Bio"), ("Experience", "1"), ("Education", "E"));
        var png = new ByteArrayContent(Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="));
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "image", "photo.png");

        var response = await client.PostAsync("/api/therapists", form);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
