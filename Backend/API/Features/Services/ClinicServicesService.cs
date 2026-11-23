using API.Common;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Services;

/// <summary>Manages the services the clinic offers (the <see cref="Service"/> entity).</summary>
public class ClinicServicesService(AppDbContext db, ImageUploads images)
{
    // The query filter hides deleted therapists from this list too.
    private IQueryable<Service> Services => db.Services.Include(s => s.Therapists);

    public async Task<List<ServiceDto>> GetAllAsync() =>
        (await Services.AsNoTracking().OrderByDescending(s => s.CreatedAt).ToListAsync()).ConvertAll(s => s.ToDto());

    public async Task<ServiceDto?> GetByIdAsync(Guid id) =>
        (await Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id))?.ToDto();

    public Task<ServiceDto> CreateAsync(ServiceDto dto, IFormFile? image = null) =>
        images.SaveWithAsync(null, image, removeImage: false, "services", async imageUrl =>
        {
            var service = new Service(dto.Name, dto.Description, imageUrl, PriceOf(dto));
            service.UpdateDetails(dto.LongDescription, dto.Duration);
            service.SetBenefits(dto.Benefits ?? []);

            db.Services.Add(service);
            await db.SaveChangesAsync();
            return service.ToDto();
        });

    public async Task<bool> UpdateAsync(Guid id, ServiceDto dto, IFormFile? image = null, bool removeImage = false)
    {
        var service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return false;

        return await images.SaveWithAsync(service.Image, image, removeImage, "services", async imageUrl =>
        {
            service.Update(dto.Name, dto.Description, imageUrl, PriceOf(dto));
            service.UpdateDetails(dto.LongDescription, dto.Duration);
            if (dto.Benefits != null)
                service.SetBenefits(dto.Benefits);

            await db.SaveChangesAsync();
            return true;
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return false;

        service.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }

    private static Money? PriceOf(ServiceDto dto) => dto.Price is { } amount ? new Money(amount, dto.Currency) : null;
}
