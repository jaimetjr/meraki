using Microsoft.AspNetCore.Http;

namespace Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task<bool> DeleteAsync(IFormFile file, string folder);
}
