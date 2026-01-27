using Application.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly string _containerName;
    private readonly string _connectionString;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration configuration, BlobContainerClient containerClient)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");

        _containerName = configuration["AzureBlobStorage:ContainerName"]!;
        _connectionString = configuration["AzureBlobStorage:ConnectionString"]!;
        _containerClient = containerClient;
        if (string.IsNullOrEmpty(_containerName) || string.IsNullOrEmpty(_connectionString))
            throw new ArgumentException("Azure Blob Storage configuration is not set properly.");
    }

    public async Task<bool> DeleteAsync(IFormFile file, string folder)
    {
        var fileName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blobClient = _containerClient.GetBlobClient(fileName);
        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var fileName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = _containerClient.GetBlobClient(fileName);
        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }
}
