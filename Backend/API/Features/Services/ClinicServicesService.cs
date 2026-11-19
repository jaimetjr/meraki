using API.Common;
using API.Features.Categories;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Services;

/// <summary>Manages the services the clinic offers (the <see cref="Service"/> entity).</summary>
public class ClinicServicesService(AppDbContext db, IFileStorageService fileStorage)
{
    private IQueryable<Service> Services => db.Services.Include(s => s.Category).Include(s => s.Benefits);

    public async Task<List<ServiceDto>> GetAllAsync() =>
        (await Services.AsNoTracking().OrderByDescending(s => s.CreatedAt).ToListAsync()).ConvertAll(s => s.ToDto());

    public async Task<ServiceDto?> GetByIdAsync(Guid id) =>
        (await Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id))?.ToDto();

    public async Task<List<ServiceDto>> GetByCategoryAsync(string categoryName) =>
        (await db.Services.AsNoTracking().Include(s => s.Category)
            .Where(s => s.Category != null && s.Category.Name == categoryName)
            .ToListAsync()).ConvertAll(s => s.ToDto());

    public async Task<ServiceDto> CreateAsync(ServiceDto dto, IFormFile? image = null)
    {
        var imageUrl = dto.Image;
        if (image is { Length: > 0 })
            imageUrl = await fileStorage.UploadAsync(image, "services");

        var service = new Service(dto.Name, dto.Description, imageUrl, new Money(dto.Price, dto.Currency));
        if (!string.IsNullOrWhiteSpace(dto.LongDescription) || !string.IsNullOrWhiteSpace(dto.Duration))
            service.UpdateDetails(dto.LongDescription, dto.Duration);
        service.UpdateCategory(await LoadCategoryAsync(dto.Category));
        service.SetBenefits(await db.Benefits.LoadAllByIdAsync(dto.Benefits?.Select(b => b.Id), "benefits"));

        db.Services.Add(service);
        await db.SaveChangesAsync();
        return service.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, ServiceDto dto, IFormFile? image = null)
    {
        var service = await Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return false;

        var imageUrl = dto.Image;
        if (image is { Length: > 0 })
            imageUrl = await fileStorage.UploadAsync(image, "services");

        service.Update(dto.Name, dto.Description, imageUrl, new Money(dto.Price, dto.Currency));
        service.UpdateDetails(dto.LongDescription, dto.Duration);
        if (dto.Category != null)
            service.UpdateCategory(await LoadCategoryAsync(dto.Category));
        if (dto.Benefits != null)
            service.SetBenefits(await db.Benefits.LoadAllByIdAsync(dto.Benefits.Select(b => b.Id), "benefits"));

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return false;

        service.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<Category> LoadCategoryAsync(CategoryDto? category) =>
        (await db.Categories.LoadAllByIdAsync([category?.Id ?? Guid.Empty], "category")).Single();
}
