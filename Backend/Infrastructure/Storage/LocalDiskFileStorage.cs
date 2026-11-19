namespace Infrastructure.Storage;

/// <summary>
/// Development storage: files live under <paramref name="rootPath"/> and are served by the API's
/// static-file middleware at <paramref name="publicBaseUrl"/>.
/// </summary>
public sealed class LocalDiskFileStorage(string rootPath, string publicBaseUrl) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(rootPath);
    private readonly string _baseUrl = publicBaseUrl.TrimEnd('/');

    public async Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default)
    {
        var key = StorageKeys.NewKey(folder, extension);
        var path = PathFor(key) ?? throw new ArgumentException($"Invalid storage folder '{folder}'.", nameof(folder));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using (var file = File.Create(path))
        {
            await content.CopyToAsync(file, cancellationToken);
        }
        return $"{_baseUrl}/{key}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        if (StorageKeys.TryGetKey(_baseUrl, url, out var key) && PathFor(key) is { } path && File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Resolves a key to a path inside the root; null for anything that would escape it.</summary>
    private string? PathFor(string key)
    {
        var path = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        return path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal) ? path : null;
    }
}
