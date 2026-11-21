using API.Common;
using Domain.Exceptions;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests;

public class ImageUploadsTests
{
    private sealed class Storage(bool canDelete) : IFileStorage
    {
        public List<string> Deleted { get; } = [];

        public Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default) =>
            Task.FromResult($"https://cdn.test/{folder}/new{extension}");

        public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            if (!canDelete) throw new IOException("storage unavailable");
            Deleted.Add(url);
            return Task.CompletedTask;
        }
    }

    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private static IFormFile PngFile() => new FormFile(new MemoryStream(Png), 0, Png.Length, "image", "photo.png");

    private const string Current = "https://cdn.test/therapists/old.png";

    [Fact]
    public async Task Failing_to_delete_the_replaced_file_does_not_fail_the_already_saved_change()
    {
        var images = new ImageUploads(new Storage(canDelete: false), NullLogger<ImageUploads>.Instance);

        var result = await images.SaveWithAsync(Current, PngFile(), removeImage: false, "therapists", url => Task.FromResult(url));

        Assert.Equal("https://cdn.test/therapists/new.png", result);
    }

    [Fact]
    public async Task A_failed_save_that_uploaded_nothing_deletes_nothing()
    {
        var storage = new Storage(canDelete: true);
        var images = new ImageUploads(storage, NullLogger<ImageUploads>.Instance);

        await Assert.ThrowsAsync<DomainException>(() =>
            images.SaveWithAsync<bool>(Current, file: null, removeImage: true, "therapists", _ => throw new DomainException("rejected")));

        Assert.Empty(storage.Deleted);
    }

    [Fact]
    public async Task A_failed_save_reports_its_own_error_even_when_the_cleanup_cannot_delete()
    {
        var images = new ImageUploads(new Storage(canDelete: false), NullLogger<ImageUploads>.Instance);

        await Assert.ThrowsAsync<DomainException>(() =>
            images.SaveWithAsync<bool>(Current, PngFile(), removeImage: false, "therapists", _ => throw new DomainException("rejected")));
    }
}
