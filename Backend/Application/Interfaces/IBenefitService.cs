using Application.DTOs;

namespace Application.Interfaces
{
    public interface IBenefitService
    {
        Task<List<BenefitDto>> GetAllAsync();
        Task<BenefitDto?> GetByIdAsync(Guid id);
        Task<List<BenefitDto>> SearchAsync(string keyword);
        Task<BenefitDto> CreateAsync(BenefitDto dto);
        Task<bool> UpdateAsync(Guid id, BenefitDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
