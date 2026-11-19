using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Testimonials;

public class TestimonialService(AppDbContext db, IFileStorageService fileStorage)
{
    public async Task<List<TestimonialDto>> GetAllAsync() =>
        (await db.Testimonials.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync()).ConvertAll(t => t.ToDto());

    public async Task<TestimonialDto?> GetByIdAsync(Guid id) =>
        (await db.Testimonials.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id))?.ToDto();

    public async Task<List<TestimonialDto>> GetTopRatedAsync(int count) =>
        (await db.Testimonials.AsNoTracking().OrderByDescending(t => t.Rating).Take(count).ToListAsync())
            .ConvertAll(t => t.ToDto());

    public async Task<TestimonialDto> CreateAsync(TestimonialDto dto, IFormFile? image = null)
    {
        var avatarUrl = dto.AuthorAvatarUrl;
        if (image is { Length: > 0 })
            avatarUrl = await fileStorage.UploadAsync(image, "testimonials");

        var testimonial = new Testimonial(dto.AuthorName, avatarUrl!, dto.Rating, dto.Content, dto.AuthorBadge);
        db.Testimonials.Add(testimonial);
        await db.SaveChangesAsync();
        return testimonial.ToDto();
    }

    // Phase 2 keeps the existing (broken) avatar handling; Phase 3 replaces it with the shared image rules.
    public async Task<bool> UpdateAsync(Guid id, TestimonialDto dto, IFormFile? image = null)
    {
        var testimonial = await db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
        if (testimonial == null) return false;

        var avatarUrl = string.Empty;
        if (image is { Length: > 0 })
            avatarUrl = await fileStorage.UploadAsync(image, "testimonials");

        testimonial.Update(dto.AuthorName, avatarUrl, dto.Rating, dto.Content, dto.AuthorBadge);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var testimonial = await db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
        if (testimonial == null) return false;

        testimonial.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }
}
