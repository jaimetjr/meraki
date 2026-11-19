using Domain.Exceptions;
using Infrastructure.Storage;

namespace API.Common;

/// <summary>The image a resource should have after a request, and the file it replaced (if any).</summary>
public sealed record ImageChange(string? Url, string? Replaced);

/// <summary>
/// The single image rule for every resource: a new file replaces the current image, <c>removeImage</c>
/// clears it, otherwise it is kept. Uploads must be real JPG/PNG/WEBP files of at most 5 MB
/// (checked by extension and by the file's signature bytes).
/// </summary>
public sealed class ImageUploads(IFileStorage storage, ILogger<ImageUploads> logger)
{
    public const long MaxBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
    };

    public async Task<ImageChange> ApplyAsync(string? currentUrl, IFormFile? file, bool removeImage, string folder)
    {
        if (file is { Length: > 0 }) return new ImageChange(await SaveAsync(file, folder), currentUrl);
        if (removeImage) return new ImageChange(null, currentUrl);
        return new ImageChange(currentUrl, null);
    }

    /// <summary>
    /// Deletes the replaced file. Call only after the new state has been saved; a failure here only
    /// leaves an unused file behind, so it is logged instead of failing the request.
    /// </summary>
    public async Task CommitAsync(ImageChange change)
    {
        if (change.Replaced is not { } replaced || replaced == change.Url) return;

        try
        {
            await storage.DeleteAsync(replaced);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not delete replaced image {Url}; it is now unused.", replaced);
        }
    }

    private async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ContentTypes.TryGetValue(extension, out var contentType))
            throw new DomainException("Only JPG, PNG or WEBP images are accepted.");
        if (file.Length > MaxBytes)
            throw new DomainException("Images must be 5 MB or smaller.");

        var header = new byte[12];
        int read;
        await using (var probe = file.OpenReadStream())
        {
            read = await probe.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        }
        if (!HasImageSignature(extension, header.AsSpan(0, read)))
            throw new DomainException("The uploaded file is not a valid image.");

        await using var content = file.OpenReadStream();
        return await storage.SaveAsync(content, extension, contentType, folder);
    }

    private static bool HasImageSignature(string extension, ReadOnlySpan<byte> header) => extension switch
    {
        ".jpg" or ".jpeg" => header.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }),
        ".png" => header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        ".webp" => header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8),
        _ => false,
    };
}
