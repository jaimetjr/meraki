using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(CategoryDto dto);
        Task<bool> UpdateAsync(Guid id, CategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
