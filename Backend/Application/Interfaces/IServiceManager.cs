using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IServiceManager
    {
        Task<List<ServiceDto>> GetAllAsync();
        Task<ServiceDto?> GetByIdAsync(Guid id);
        Task<List<ServiceDto>> GetByCategoryAsync(string category);
        Task<ServiceDto> CreateAsync(ServiceDto dto, IFormFile? image = null);
        Task<bool> UpdateAsync(Guid id, ServiceDto dto, IFormFile? image = null);
        Task<bool> DeleteAsync(Guid id);

    }
}
