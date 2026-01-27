using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class TestimonialService : ITestimonialService
    {
        private readonly ITestimonialRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public TestimonialService(ITestimonialRepository repo, IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<List<TestimonialDto>> GetAllAsync() =>
            _mapper.Map<List<TestimonialDto>>(await _repo.GetAllAsync());

        public async Task<TestimonialDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TestimonialDto>(entity);
        }

        public async Task<List<TestimonialDto>> GetTopRatedAsync(int count)
        {
            var top = await _repo.GetTopRatedAsync(count);
            return _mapper.Map<List<TestimonialDto>>(top);
        }

        public async Task<TestimonialDto> CreateAsync(TestimonialDto dto, IFormFile? image = null)
        {
            string? avatarUrl = dto.AuthorAvatarUrl;
            
            // Upload image if provided
            if (image != null && image.Length > 0)
            {
                avatarUrl = await _fileStorageService.UploadAsync(image, "testimonials");
            }
            
            // Create Testimonial entity using constructor
            var testimonial = new Testimonial(
                dto.AuthorName,
                avatarUrl,
                dto.Rating,
                dto.Content,
                dto.AuthorBadge
            );
            
            await _repo.AddAsync(testimonial);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TestimonialDto>(testimonial);
        }

        public async Task<bool> UpdateAsync(Guid id, TestimonialDto dto, IFormFile? image = null)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            string? avatarUrl = string.Empty;
            
            // Upload new image if provided, otherwise use existing URL from DTO
            if (image != null && image.Length > 0)
            {
                avatarUrl = await _fileStorageService.UploadAsync(image, "testimonials");
            }

            existing.Update(dto.AuthorName, avatarUrl, dto.Rating, dto.Content, dto.AuthorBadge);
            await _repo.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _repo.ExistsAsync(id);
            if (!exists) return false;

            await _repo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

}
