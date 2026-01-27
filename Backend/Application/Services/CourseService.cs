using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public CourseService(ICourseRepository courseRepo, IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            _courseRepo = courseRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseRepo.GetAllAsync();
            return _mapper.Map<List<CourseDto>>(courses);
        }

        public async Task<CourseDto?> GetByIdAsync(Guid id)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            return course == null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task<List<CourseDto>> GetByStatusAsync(string status)
        {
            if (!Enum.TryParse<CourseStatus>(status, out var parsedStatus))
                throw new ArgumentException($"Invalid course status: {status}");

            var courses = await _courseRepo.GetByStatusAsync(parsedStatus);
            return _mapper.Map<List<CourseDto>>(courses);
        }

        public async Task<CourseDto> CreateAsync(CourseDto dto, IFormFile? image = null)
        {
            string? imageUrl = dto.Image;
            
            // Upload image if provided
            if (image != null && image.Length > 0)
            {
                imageUrl = await _fileStorageService.UploadAsync(image, "courses");
            }

            var course = new Course(
                dto.Title,
                dto.Description,
                imageUrl,
                dto.Instructor,
                Enum.Parse<CourseType>(dto.Type),
                Enum.Parse<CourseStatus>(dto.Status)
            );

            if (dto.StartDate.HasValue && !string.IsNullOrEmpty(dto.Modality))
            {
                var modality = Enum.Parse<Modality>(dto.Modality);
                var price = new Money(dto.Price ?? 0, dto.Currency);
                // Schedule method will handle UTC conversion
                course.Schedule(dto.StartDate.Value, dto.EndDate, modality, price, dto.Link);
            }

            await _courseRepo.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<bool> UpdateAsync(Guid id, CourseDto dto, IFormFile? image = null)
        {
            var existing = await _courseRepo.GetByIdAsync(id);
            if (existing == null) return false;

            string? imageUrl = dto.Image;
            
            // Upload new image if provided, otherwise use existing URL from DTO
            if (image != null && image.Length > 0)
            {
                imageUrl = await _fileStorageService.UploadAsync(image, "courses");
            }

            existing.Update(
                dto.Title,
                dto.Description,
                imageUrl,
                dto.Instructor,
                Enum.Parse<CourseType>(dto.Type),
                Enum.Parse<CourseStatus>(dto.Status)
            );

            if (dto.StartDate.HasValue && !string.IsNullOrEmpty(dto.Modality))
            {
                var modality = Enum.Parse<Modality>(dto.Modality);
                var price = new Money(dto.Price ?? 0, dto.Currency);
                // Schedule method will handle UTC conversion
                existing.Schedule(dto.StartDate.Value, dto.EndDate, modality, price, dto.Link);
            }

            await _courseRepo.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _courseRepo.ExistsAsync(id);
            if (!exists) return false;

            await _courseRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
