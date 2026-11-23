using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Tests;

/// <summary>
/// A request that uploads an image and then fails (bad date, unknown relation id, …) must not leave the
/// new file in storage, and must never delete the image the resource already had.
/// </summary>
public class ImageCleanupTests(ApiFactory api)
{
    private sealed class RecordingStorage : IFileStorage
    {
        public ConcurrentQueue<string> Saved { get; } = new();
        public ConcurrentQueue<string> Deleted { get; } = new();

        public Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default)
        {
            var url = $"https://cdn.test/{folder}/{Guid.NewGuid():N}{extension}";
            Saved.Enqueue(url);
            return Task.FromResult(url);
        }

        public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            Deleted.Enqueue(url);
            return Task.CompletedTask;
        }
    }

    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private static MultipartFormDataContent WithPng(MultipartFormDataContent form)
    {
        var file = new ByteArrayContent(Png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "image", "photo.png");
        return form;
    }

    private static string UnknownIds() => JsonSerializer.Serialize(new[] { new { id = Guid.NewGuid() } });

    private async Task<(HttpClient Client, RecordingStorage Storage, WebApplicationFactory<Program> App)> AdminWithRecordingStorageAsync()
    {
        var storage = new RecordingStorage();
        var app = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFileStorage>();
            services.AddSingleton<IFileStorage>(storage);
        }));
        var (_, email) = await api.CreateUserAsync(UserRole.Admin);
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await api.LoginAsync(email, ApiFactory.Password));
        return (client, storage, app);
    }

    [Fact]
    public async Task A_course_rejected_after_its_image_was_uploaded_leaves_no_file_behind()
    {
        var (client, storage, app) = await AdminWithRecordingStorageAsync();
        await using var _ = app;

        var response = await client.PostAsync("/api/courses", WithPng(Http.Form(
            ("Title", Http.Unique("C")), ("Description", "Desc"), ("Instructor", "Ana"), ("Type", "Curso"), ("Status", "EmBreve"),
            ("StartDate", "2020-01-01T10:00:00Z"), ("Modality", "Online"), ("Price", "10"), ("Currency", "BRL"))));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var uploaded = Assert.Single(storage.Saved);
        Assert.Contains(uploaded, storage.Deleted);
    }

    [Fact]
    public async Task A_therapist_rejected_for_an_unknown_service_leaves_no_file_behind()
    {
        var (client, storage, app) = await AdminWithRecordingStorageAsync();
        await using var _ = app;

        var response = await client.PostAsync("/api/therapists", WithPng(Http.Form(
            ("Name", Http.Unique("T")), ("Bio", "Bio"), ("Experience", "1"), ("Education", "E"), ("services", UnknownIds()))));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var uploaded = Assert.Single(storage.Saved);
        Assert.Contains(uploaded, storage.Deleted);
    }

    [Fact]
    public async Task A_therapist_update_rejected_for_an_unknown_service_removes_the_new_file_and_keeps_the_current_one()
    {
        var (client, storage, app) = await AdminWithRecordingStorageAsync();
        await using var _ = app;
        static MultipartFormDataContent Therapist() =>
            Http.Form(("Name", Http.Unique("T")), ("Bio", "Bio"), ("Experience", "1"), ("Education", "E"));
        var id = (await (await client.PostAsync("/api/therapists", WithPng(Therapist()))).Json()).Id();
        var current = Assert.Single(storage.Saved);

        var form = WithPng(Therapist());
        form.Add(new StringContent(UnknownIds()), "services");
        var response = await client.PutAsync($"/api/therapists/{id}", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var uploaded = storage.Saved.Last();
        Assert.Contains(uploaded, storage.Deleted);
        Assert.DoesNotContain(current, storage.Deleted);
        var therapist = await client.GetJson($"/api/therapists/{id}");
        Assert.Equal(current, therapist.Str("image"));
    }
}
