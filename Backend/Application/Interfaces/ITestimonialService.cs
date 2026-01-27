using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface ITestimonialService
    {
        Task<List<TestimonialDto>> GetAllAsync();
        Task<TestimonialDto?> GetByIdAsync(Guid id);
        Task<List<TestimonialDto>> GetTopRatedAsync(int count);
        Task<TestimonialDto> CreateAsync(TestimonialDto dto, IFormFile? image = null);
        Task<bool> UpdateAsync(Guid id, TestimonialDto dto, IFormFile? image = null);
        Task<bool> DeleteAsync(Guid id);
    }

}
