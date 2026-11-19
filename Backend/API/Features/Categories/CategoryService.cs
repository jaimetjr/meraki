using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Categories;

public class CategoryService(AppDbContext db)
{
    public async Task<List<CategoryDto>> GetAllAsync() =>
        (await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync()).ConvertAll(c => c.ToDto());

    public async Task<CategoryDto?> GetByIdAsync(Guid id) =>
        (await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id))?.ToDto();

    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        var category = new Category(dto.Name);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, CategoryDto dto)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return false;

        category.Update(dto.Name);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Soft-deletes the category; refused while any active service still uses it.</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return false;

        var usedBy = await db.Services.CountAsync(s => s.CategoryId == id);
        if (usedBy > 0)
            throw new ConflictException($"Category is used by {usedBy} service(s); move them to another category first.");

        category.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }
}
