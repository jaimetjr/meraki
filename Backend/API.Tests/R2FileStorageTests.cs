using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Infrastructure.Storage;

namespace API.Tests;

/// <summary>Runs Adobe S3Mock, an S3-compatible server, once for the R2 storage tests.</summary>
public sealed class S3Fixture : IAsyncLifetime
{
    public const string AccessKey = "test-access-key";
    public const string SecretKey = "test-secret-key";
    public const string Bucket = "meraki-test";

    private readonly IContainer _s3 = new ContainerBuilder()
        .WithImage("adobe/s3mock:3.12.0")
        .WithEnvironment("initialBuckets", Bucket)
        .WithPortBinding(9090, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/").ForPort(9090)))
        .Build();

    public string ServiceUrl => $"http://{_s3.Hostname}:{_s3.GetMappedPublicPort(9090)}";

    public AmazonS3Client Client() => new(new BasicAWSCredentials(AccessKey, SecretKey),
        new AmazonS3Config { ServiceURL = ServiceUrl, ForcePathStyle = true, AuthenticationRegion = "auto" });

    public ValueTask InitializeAsync() => new(_s3.StartAsync());

    public ValueTask DisposeAsync() => _s3.DisposeAsync();
}

/// <summary>
/// Exercises R2FileStorage against a real S3 API. R2-specific behavior (unsigned payloads over
/// HTTPS) can only be verified against a real bucket.
/// </summary>
public class R2FileStorageTests(S3Fixture s3) : IClassFixture<S3Fixture>
{
    private const string PublicBaseUrl = "https://images.example.test";
    private static readonly byte[] Bytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3];

    private R2FileStorage Storage() => new(new R2Options
    {
        AccessKeyId = S3Fixture.AccessKey,
        SecretAccessKey = S3Fixture.SecretKey,
        Bucket = S3Fixture.Bucket,
        ServiceUrl = s3.ServiceUrl,
    }, PublicBaseUrl);

    private static string KeyOf(string url) => url[(PublicBaseUrl.Length + 1)..];

    [Fact]
    public async Task Saved_file_lands_in_the_bucket_under_its_folder_with_its_content_type()
    {
        using var storage = Storage();

        var url = await storage.SaveAsync(new MemoryStream(Bytes), ".png", "image/png", "therapists");

        Assert.StartsWith($"{PublicBaseUrl}/therapists/", url);
        Assert.EndsWith(".png", url);
        using var client = s3.Client();
        using var stored = await client.GetObjectAsync(S3Fixture.Bucket, KeyOf(url));
        Assert.Equal("image/png", stored.Headers.ContentType);
        using var content = new MemoryStream();
        await stored.ResponseStream.CopyToAsync(content);
        Assert.Equal(Bytes, content.ToArray());
    }

    [Fact]
    public async Task Delete_removes_the_object()
    {
        using var storage = Storage();
        var url = await storage.SaveAsync(new MemoryStream(Bytes), ".png", "image/png", "services");

        await storage.DeleteAsync(url);

        using var client = s3.Client();
        var error = await Assert.ThrowsAnyAsync<AmazonS3Exception>(() => client.GetObjectAsync(S3Fixture.Bucket, KeyOf(url)));
        Assert.Equal(System.Net.HttpStatusCode.NotFound, error.StatusCode);
    }

    [Fact]
    public async Task Delete_ignores_urls_it_does_not_own()
    {
        using var storage = Storage();
        var url = await storage.SaveAsync(new MemoryStream(Bytes), ".png", "image/png", "courses");

        await storage.DeleteAsync($"https://elsewhere.example/{KeyOf(url)}");

        using var client = s3.Client();
        using var stillThere = await client.GetObjectAsync(S3Fixture.Bucket, KeyOf(url));
        Assert.Equal(System.Net.HttpStatusCode.OK, stillThere.HttpStatusCode);
    }
}
