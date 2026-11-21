using Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;

namespace API.Common;

/// <summary>
/// Chooses where uploaded images live from configuration:
/// <c>Storage:Provider</c> = <c>Local</c> (default; files on disk, served by this API) or <c>R2</c> (Cloudflare R2).
/// <c>Storage:PublicBaseUrl</c> is the URL prefix browsers use to load the files. It is required for both
/// providers and has no default (Development sets it in appsettings.Development.json), so a deploy that
/// forgets it fails at startup instead of storing image URLs that point at localhost.
/// </summary>
public static class StorageSetup
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<ImageUploads>();

        switch (Provider(configuration))
        {
            case "local":
                services.AddSingleton<IFileStorage>(new LocalDiskFileStorage(LocalRoot(configuration, environment), PublicBaseUrl(configuration)));
                break;
            case "r2":
                var options = configuration.GetSection("Storage:R2").Get<R2Options>()
                    ?? throw new InvalidOperationException("Storage:R2 settings are missing.");
                services.AddSingleton<IFileStorage>(new R2FileStorage(options, PublicBaseUrl(configuration)));
                break;
            default:
                throw new InvalidOperationException($"Unknown Storage:Provider '{configuration["Storage:Provider"]}' (use Local or R2).");
        }

        return services;
    }

    /// <summary>With local storage, serves the upload folder at the path of Storage:PublicBaseUrl.</summary>
    public static WebApplication UseLocalUploads(this WebApplication app)
    {
        if (Provider(app.Configuration) != "local") return app;

        var root = LocalRoot(app.Configuration, app.Environment);
        Directory.CreateDirectory(root);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = new Uri(PublicBaseUrl(app.Configuration)).AbsolutePath.TrimEnd('/'),
        });
        return app;
    }

    private static string PublicBaseUrl(IConfiguration configuration)
    {
        var url = configuration["Storage:PublicBaseUrl"];
        return string.IsNullOrWhiteSpace(url)
            ? throw new InvalidOperationException(
                "Storage:PublicBaseUrl is required: the URL prefix browsers load images from (the R2 bucket's public URL, or this API's /uploads URL for local storage).")
            : url;
    }

    private static string Provider(IConfiguration configuration) =>
        (configuration["Storage:Provider"] ?? "Local").ToLowerInvariant();

    private static string LocalRoot(IConfiguration configuration, IHostEnvironment environment) =>
        Path.GetFullPath(configuration["Storage:Local:RootPath"]
            ?? Path.Combine(environment.ContentRootPath, "wwwroot", "uploads"));
}
