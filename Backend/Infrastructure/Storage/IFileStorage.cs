namespace Infrastructure.Storage;

/// <summary>Stores public files (images) and hands back the URL browsers load them from.</summary>
public interface IFileStorage
{
    /// <summary>Saves the content as <c>{folder}/{new-guid}{extension}</c> and returns its public URL.</summary>
    Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default);

    /// <summary>Deletes a file previously returned by <see cref="SaveAsync"/>. URLs this storage does not own are ignored.</summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}

internal static class StorageKeys
{
    public static string NewKey(string folder, string extension) => $"{folder}/{Guid.NewGuid():N}{extension}";

    /// <summary>Extracts the object key from a URL produced under <paramref name="publicBaseUrl"/>.</summary>
    public static bool TryGetKey(string publicBaseUrl, string url, out string key)
    {
        var prefix = publicBaseUrl.TrimEnd('/') + "/";
        key = url.StartsWith(prefix, StringComparison.Ordinal) ? url[prefix.Length..] : string.Empty;
        return key.Length > 0;
    }
}
