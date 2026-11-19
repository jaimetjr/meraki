using API.Common;
using Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests;

public class ImageUploadsTests
{
    private sealed class StorageThatCannotDelete : IFileStorage
    {
        public Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default) =>
            Task.FromResult($"https://cdn.test/{folder}/new{extension}");

        public Task DeleteAsync(string url, CancellationToken cancellationToken = default) =>
            throw new IOException("storage unavailable");
    }

    [Fact]
    public async Task Failing_to_delete_the_replaced_file_does_not_fail_the_already_saved_change()
    {
        var images = new ImageUploads(new StorageThatCannotDelete(), NullLogger<ImageUploads>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            images.CommitAsync(new ImageChange("https://cdn.test/therapists/new.png", "https://cdn.test/therapists/old.png")));

        Assert.Null(exception);
    }
}
