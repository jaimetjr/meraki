using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task<bool> DeleteAsync(IFormFile file, string folder);
}
