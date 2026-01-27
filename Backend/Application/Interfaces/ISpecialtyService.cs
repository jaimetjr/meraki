using Application.DTOs;

namespace Application.Interfaces
{
    public interface ISpecialtyService
    {
        Task<List<SpecialtyDto>> GetAllAsync();
        Task<SpecialtyDto?> GetByIdAsync(Guid id);
        Task<SpecialtyDto?> GetByNameAsync(string name);
        Task<SpecialtyDto> CreateAsync(SpecialtyDto dto);
        Task<bool> UpdateAsync(Guid id, SpecialtyDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
