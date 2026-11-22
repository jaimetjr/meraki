using Domain.Exceptions;
using Infrastructure.Storage;

namespace API.Common;

/// <summary>
/// The image a resource should have after a request, the file it replaced (if any), and the file this
/// request uploaded (if any).
/// </summary>
internal sealed record ImageChange(string? Url, string? Replaced, string? Uploaded);

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

    /// <summary>
    /// Applies the image rule, then runs <paramref name="save"/> with the image URL the resource should have
    /// (build or update the entity and save it there).
    /// <list type="bullet">
    /// <item>If <paramref name="save"/> throws, the file this request uploaded is deleted, so a rejected
    /// request leaves nothing behind. The current image is never touched.</item>
    /// <item>If it succeeds, the replaced file is deleted.</item>
    /// </list>
    /// </summary>
    public async Task<T> SaveWithAsync<T>(string? currentUrl, IFormFile? file, bool removeImage, string folder, Func<string?, Task<T>> save)
    {
        var change = await ApplyAsync(currentUrl, file, removeImage, folder);
        T result;
        try
        {
            result = await save(change.Url);
        }
        catch
        {
            if (change.Uploaded is { } uploaded) await DeleteQuietlyAsync(uploaded, "upload of a rejected request");
            throw;
        }

        if (change.Replaced is { } replaced && replaced != change.Url) await DeleteQuietlyAsync(replaced, "replaced image");
        return result;
    }

    private async Task<ImageChange> ApplyAsync(string? currentUrl, IFormFile? file, bool removeImage, string folder)
    {
        if (file is { Length: > 0 })
        {
            var uploaded = await SaveAsync(file, folder);
            return new ImageChange(uploaded, currentUrl, uploaded);
        }
        if (removeImage) return new ImageChange(null, currentUrl, null);
        return new ImageChange(currentUrl, null, null);
    }

    /// <summary>A failed delete only leaves an unused file behind, so it is logged instead of failing the request.</summary>
    private async Task DeleteQuietlyAsync(string url, string what)
    {
        try
        {
            await storage.DeleteAsync(url);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not delete {What} {Url}; it is now unused.", what, url);
        }
    }

    private async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ContentTypes.TryGetValue(extension, out var contentType))
            throw new DomainException("Apenas imagens JPG, PNG ou WEBP são aceitas.");
        if (file.Length > MaxBytes)
            throw new DomainException("A imagem deve ter no máximo 5 MB.");

        var header = new byte[12];
        int read;
        await using (var probe = file.OpenReadStream())
        {
            read = await probe.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        }
        if (!HasImageSignature(extension, header.AsSpan(0, read)))
            throw new DomainException("O arquivo enviado não é uma imagem válida.");

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
