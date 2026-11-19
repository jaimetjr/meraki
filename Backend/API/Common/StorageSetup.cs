using Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;

namespace API.Common;

/// <summary>
/// Chooses where uploaded images live from configuration:
/// <c>Storage:Provider</c> = <c>Local</c> (default; files on disk, served by this API) or <c>R2</c> (Cloudflare R2).
/// <c>Storage:PublicBaseUrl</c> is the URL prefix browsers use to load the files.
/// </summary>
public static class StorageSetup
{
    private const string DefaultLocalBaseUrl = "http://localhost:5063/uploads";

    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddScoped<ImageUploads>();

        switch (Provider(configuration))
        {
            case "local":
                var root = LocalRoot(configuration, environment);
                var localBaseUrl = configuration["Storage:PublicBaseUrl"] ?? DefaultLocalBaseUrl;
                services.AddSingleton<IFileStorage>(new LocalDiskFileStorage(root, localBaseUrl));
                break;
            case "r2":
                var options = configuration.GetSection("Storage:R2").Get<R2Options>()
                    ?? throw new InvalidOperationException("Storage:R2 settings are missing.");
                var publicBaseUrl = configuration["Storage:PublicBaseUrl"]
                    ?? throw new InvalidOperationException("Storage:PublicBaseUrl is required for R2 (the bucket's public URL).");
                services.AddSingleton<IFileStorage>(new R2FileStorage(options, publicBaseUrl));
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
        var baseUrl = app.Configuration["Storage:PublicBaseUrl"] ?? DefaultLocalBaseUrl;
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = new Uri(baseUrl).AbsolutePath.TrimEnd('/'),
        });
        return app;
    }

    private static string Provider(IConfiguration configuration) =>
        (configuration["Storage:Provider"] ?? "Local").ToLowerInvariant();

    private static string LocalRoot(IConfiguration configuration, IHostEnvironment environment) =>
        Path.GetFullPath(configuration["Storage:Local:RootPath"]
            ?? Path.Combine(environment.ContentRootPath, "wwwroot", "uploads"));
}
