using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly IServiceRepository _serviceRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IBenefitRepository _benefitRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public ServiceManager(IServiceRepository serviceRepo, ICategoryRepository categoryRepo, IBenefitRepository benefitRepo, IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            _serviceRepo = serviceRepo;
            _categoryRepo = categoryRepo;
            _benefitRepo = benefitRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<List<ServiceDto>> GetAllAsync()
        {
            var services = await _serviceRepo.GetAllAsync();
            return _mapper.Map<List<ServiceDto>>(services);
        }

        public async Task<ServiceDto?> GetByIdAsync(Guid id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            return service == null ? null : _mapper.Map<ServiceDto>(service);
        }

        public async Task<List<ServiceDto>> GetByCategoryAsync(string category)
        {
            var services = await _serviceRepo.GetByCategoryAsync(category);
            return _mapper.Map<List<ServiceDto>>(services);
        }

        public async Task<ServiceDto> CreateAsync(ServiceDto dto, IFormFile? image = null)
        {
            string? imageUrl = dto.Image;
            
            // Upload image if provided
            if (image != null && image.Length > 0)
            {
                imageUrl = await _fileStorageService.UploadAsync(image, "services");
            }
            
            // Create Money value object from DTO
            var price = new Money(dto.Price, dto.Currency);
            
            // Create Service entity using constructor
            var service = new Service(
                dto.Name,
                dto.Description,
                imageUrl,
                price
            );
            
            // Handle optional properties
            if (!string.IsNullOrWhiteSpace(dto.LongDescription) || !string.IsNullOrWhiteSpace(dto.Duration))
            {
                service.UpdateDetails(dto.LongDescription, dto.Duration);
            }
            
            // Handle Category relationship
            if (dto.Category != null)
            {
                Category? category = null;
                if (dto.Category.Id != Guid.Empty)
                {
                    // Load existing category
                    category = await _categoryRepo.GetByIdAsync(dto.Category.Id);
                    if (category == null)
                    {
                        throw new InvalidOperationException($"Category with ID {dto.Category.Id} does not exist.");
                    }
                }
                
                if (category == null)
                {
                    // Create new category and add to repository for proper tracking
                    category = new Category(dto.Category.Name);
                    await _categoryRepo.AddAsync(category);
                    // Save the new category first to get its ID
                    await _unitOfWork.SaveChangesAsync();
                    // After SaveChanges, EF Core sets the category's ID
                    // Verify the category has an ID before proceeding
                    if (category.Id == Guid.Empty)
                    {
                        throw new InvalidOperationException("Failed to create category - category ID was not generated.");
                    }
                }
                
                service.UpdateCategory(category);
            }
            else
            {
                throw new InvalidOperationException("Category is required for Service creation.");
            }
            
            // Handle Benefits relationship
            if (dto.Benefits != null)
            {
                foreach (var benefitDto in dto.Benefits)
                {
                    Benefit? benefit = null;
                    if (benefitDto.Id != Guid.Empty)
                    {
                        // Load existing benefit
                        benefit = await _benefitRepo.GetByIdAsync(benefitDto.Id);
                    }
                    
                    if (benefit == null)
                    {
                        // Create new benefit and add to repository for proper tracking
                        benefit = new Benefit(benefitDto.Title, benefitDto.Description);
                        await _benefitRepo.AddAsync(benefit);
                    }
                    
                    service.AddBenefit(benefit);
                }
            }
            
            await _serviceRepo.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ServiceDto>(service);
        }

        public async Task<bool> UpdateAsync(Guid id, ServiceDto dto, IFormFile? image = null)
        {
            var existing = await _serviceRepo.GetByIdAsync(id);
            if (existing == null) return false;

            string? imageUrl = dto.Image;
            
            // Upload new image if provided, otherwise use existing URL from DTO
            if (image != null && image.Length > 0)
            {
                imageUrl = await _fileStorageService.UploadAsync(image, "services");
            }

            existing.Update(dto.Name, dto.Description, imageUrl, new Money(dto.Price, dto.Currency));
            existing.UpdateDetails(dto.LongDescription, dto.Duration);

            if (dto.Category != null)
            {
                Category? category = null;
                if (dto.Category.Id != Guid.Empty)
                {
                    // Load existing category
                    category = await _categoryRepo.GetByIdAsync(dto.Category.Id);
                    if (category == null)
                    {
                        throw new InvalidOperationException($"Category with ID {dto.Category.Id} does not exist.");
                    }
                }
                
                if (category == null)
                {
                    // Create new category and add to repository for proper tracking
                    if (string.IsNullOrWhiteSpace(dto.Category.Name))
                    {
                        throw new InvalidOperationException("Category name is required when creating a new category.");
                    }
                    category = new Category(dto.Category.Name);
                    await _categoryRepo.AddAsync(category);
                    // Save the new category first to get its ID
                    await _unitOfWork.SaveChangesAsync();
                    // After SaveChanges, EF Core sets the category's ID
                    // Verify the category has an ID before proceeding
                    if (category.Id == Guid.Empty)
                    {
                        throw new InvalidOperationException("Failed to create category - category ID was not generated.");
                    }
                }
                
                existing.UpdateCategory(category);
            }
            // If Category is not provided in update, keep the existing category

            if (dto.Benefits != null)
            {
                foreach (var benefitDto in dto.Benefits)
                {
                    Benefit? benefit = null;
                    if (benefitDto.Id != Guid.Empty)
                    {
                        // Load existing benefit
                        benefit = await _benefitRepo.GetByIdAsync(benefitDto.Id);
                    }
                    
                    if (benefit == null)
                    {
                        // Create new benefit and add to repository for proper tracking
                        benefit = new Benefit(benefitDto.Title, benefitDto.Description);
                        await _benefitRepo.AddAsync(benefit);
                    }
                    
                    existing.AddBenefit(benefit);
                }
            }

            await _serviceRepo.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _serviceRepo.ExistsAsync(id);
            if (!exists) return false;

            await _serviceRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
