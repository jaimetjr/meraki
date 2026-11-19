using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Benefits;

public class BenefitService(AppDbContext db)
{
    public async Task<List<BenefitDto>> GetAllAsync() =>
        (await db.Benefits.AsNoTracking().OrderByDescending(b => b.CreatedAt).ToListAsync()).ConvertAll(b => b.ToDto());

    public async Task<BenefitDto?> GetByIdAsync(Guid id) =>
        (await db.Benefits.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id))?.ToDto();

    public async Task<List<BenefitDto>> SearchAsync(string keyword) =>
        (await db.Benefits.AsNoTracking()
            .Where(b => b.Title.Contains(keyword) || (b.Description != null && b.Description.Contains(keyword)))
            .ToListAsync()).ConvertAll(b => b.ToDto());

    public async Task<BenefitDto> CreateAsync(BenefitDto dto)
    {
        var benefit = new Benefit(dto.Title, dto.Description);
        db.Benefits.Add(benefit);
        await db.SaveChangesAsync();
        return benefit.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, BenefitDto dto)
    {
        var benefit = await db.Benefits.FirstOrDefaultAsync(b => b.Id == id);
        if (benefit == null) return false;

        benefit.Update(dto.Title, dto.Description);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var benefit = await db.Benefits.FirstOrDefaultAsync(b => b.Id == id);
        if (benefit == null) return false;

        benefit.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }
}
