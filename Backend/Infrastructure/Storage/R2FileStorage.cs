using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Infrastructure.Storage;

public sealed class R2Options
{
    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;

    /// <summary>Overrides the endpoint (defaults to https://{AccountId}.r2.cloudflarestorage.com); used by tests.</summary>
    public string? ServiceUrl { get; set; }
}

/// <summary>
/// Production storage on Cloudflare R2 (S3-compatible). Objects are uploaded with the S3 API and
/// read by browsers from the bucket's public URL (custom domain or r2.dev), <paramref name="publicBaseUrl"/>.
/// </summary>
public sealed class R2FileStorage : IFileStorage, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;
    private readonly string _baseUrl;
    private readonly bool _isHttps;

    public R2FileStorage(R2Options options, string publicBaseUrl)
    {
        var serviceUrl = options.ServiceUrl ?? $"https://{options.AccountId}.r2.cloudflarestorage.com";
        _client = new AmazonS3Client(
            new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey),
            new AmazonS3Config { ServiceURL = serviceUrl, ForcePathStyle = true, AuthenticationRegion = "auto" });
        _bucket = options.Bucket;
        _baseUrl = publicBaseUrl.TrimEnd('/');
        _isHttps = serviceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> SaveAsync(Stream content, string extension, string contentType, string folder, CancellationToken cancellationToken = default)
    {
        var key = StorageKeys.NewKey(folder, extension);
        await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
            // R2 does not support the SDK's streaming SigV4 payload signing or its default
            // trailing checksums (Cloudflare's AWS SDK for .NET guidance). Unsigned payloads are
            // only allowed over HTTPS, which R2 always uses.
            DisablePayloadSigning = _isHttps,
            DisableDefaultChecksumValidation = true,
        }, cancellationToken);
        return $"{_baseUrl}/{key}";
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        if (StorageKeys.TryGetKey(_baseUrl, url, out var key))
            await _client.DeleteObjectAsync(_bucket, key, cancellationToken);
    }

    public void Dispose() => _client.Dispose();
}
