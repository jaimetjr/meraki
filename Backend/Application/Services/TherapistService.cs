using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TherapistService : ITherapistService
    {
        private readonly ITherapistRepository _therapistRepo;
        private readonly ISpecialtyRepository _specialtyRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public TherapistService(ITherapistRepository therapistRepo, ISpecialtyRepository specialtyRepo, IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            _therapistRepo = therapistRepo;
            _specialtyRepo = specialtyRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<List<TherapistDto>> GetAllAsync()
        {
            var therapists = await _therapistRepo.GetAllAsync();
            return _mapper.Map<List<TherapistDto>>(therapists);
        }

        public async Task<TherapistDto?> GetByIdAsync(Guid id)
        {
            var therapist = await _therapistRepo.GetByIdAsync(id);
            return therapist == null ? null : _mapper.Map<TherapistDto>(therapist);
        }

        public async Task<List<TherapistDto>> FindBySpecialtyAsync(string specialtyName)
        {
            var therapists = await _therapistRepo.FindBySpecialtyAsync(specialtyName);
            return _mapper.Map<List<TherapistDto>>(therapists);
        }

        public async Task<TherapistDto> CreateAsync(TherapistDto dto, IFormFile? image = null)
        {
            string? imageUrl = dto.Image;
            
            // Upload image if provided
            if (image != null && image.Length > 0)
            {
                imageUrl = await _fileStorageService.UploadAsync(image, "therapists");
            }
            
            // Create Therapist entity using constructor
            var therapist = new Therapist(
                dto.Name,
                dto.Bio,
                imageUrl,
                dto.Experience,
                dto.Education
            );
            
            // Handle optional Specialties collection
            if (dto.Specialties != null)
            {
                foreach (var specialtyDto in dto.Specialties)
                {
                    Specialty? specialty = null;
                    if (specialtyDto.Id != Guid.Empty)
                    {
                        // Load existing specialty
                        specialty = await _specialtyRepo.GetByIdAsync(specialtyDto.Id);
                    }
                    
                    if (specialty == null)
                    {
                        // Create new specialty and add to repository for proper tracking
                        specialty = new Specialty(specialtyDto.Name, specialtyDto.Description);
                        await _specialtyRepo.AddAsync(specialty);
                    }
                    
                    therapist.AddSpecialty(specialty);
                }
            }
            
            await _therapistRepo.AddAsync(therapist);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TherapistDto>(therapist);
        }

        public async Task<bool> UpdateAsync(Guid id, TherapistDto dto, IFormFile? image = null)
        {
            var existing = await _therapistRepo.GetByIdAsync(id);
            if (existing == null) return false;

            string? imageUrl = string.Empty;
            
            // Upload new image if provided, otherwise use existing URL from DTO
            if (image != null && image.Length > 0)
            {
                imageUrl = await _fileStorageService.UploadAsync(image, "therapists");
            } 
            else 
            {
                imageUrl = existing.Image;
            }

            existing.Update(dto.Name, dto.Bio, imageUrl, dto.Experience, dto.Education);

            if (dto.Specialties != null)
            {
                foreach (var specialtyDto in dto.Specialties)
                {
                    Specialty? specialty = null;
                    if (specialtyDto.Id != Guid.Empty)
                    {
                        // Load existing specialty
                        specialty = await _specialtyRepo.GetByIdAsync(specialtyDto.Id);
                    }
                    
                    if (specialty == null)
                    {
                        // Create new specialty and add to repository for proper tracking
                        specialty = new Specialty(specialtyDto.Name, specialtyDto.Description);
                        await _specialtyRepo.AddAsync(specialty);
                    }
                    
                    existing.AddSpecialty(specialty);
                }
            }

            await _therapistRepo.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _therapistRepo.ExistsAsync(id);
            if (!exists) return false;

            await _therapistRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

    }
}
