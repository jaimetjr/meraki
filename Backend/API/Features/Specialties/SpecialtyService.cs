using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Specialties;

public class SpecialtyService(AppDbContext db)
{
    public async Task<List<SpecialtyDto>> GetAllAsync() =>
        (await db.Specialties.AsNoTracking().OrderByDescending(s => s.CreatedAt).ToListAsync()).ConvertAll(s => s.ToDto());

    public async Task<SpecialtyDto?> GetByIdAsync(Guid id) =>
        (await db.Specialties.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id))?.ToDto();

    public async Task<SpecialtyDto?> GetByNameAsync(string name) =>
        (await db.Specialties.AsNoTracking().FirstOrDefaultAsync(s => s.Name == name))?.ToDto();

    public async Task<SpecialtyDto> CreateAsync(SpecialtyDto dto)
    {
        var specialty = new Specialty(dto.Name, dto.Description);
        db.Specialties.Add(specialty);
        await db.SaveChangesAsync();
        return specialty.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, SpecialtyDto dto)
    {
        var specialty = await db.Specialties.FirstOrDefaultAsync(s => s.Id == id);
        if (specialty == null) return false;

        specialty.Update(dto.Name, dto.Description);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var specialty = await db.Specialties.FirstOrDefaultAsync(s => s.Id == id);
        if (specialty == null) return false;

        specialty.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }
}
