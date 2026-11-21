using API.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Testimonials;

public class TestimonialService(AppDbContext db, ImageUploads images)
{
    public async Task<List<TestimonialDto>> GetAllAsync() =>
        (await db.Testimonials.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync()).ConvertAll(t => t.ToDto());

    public async Task<TestimonialDto?> GetByIdAsync(Guid id) =>
        (await db.Testimonials.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id))?.ToDto();

    public async Task<List<TestimonialDto>> GetTopRatedAsync(int count) =>
        (await db.Testimonials.AsNoTracking().OrderByDescending(t => t.Rating).Take(count).ToListAsync())
            .ConvertAll(t => t.ToDto());

    public Task<TestimonialDto> CreateAsync(TestimonialDto dto, IFormFile? image = null) =>
        images.SaveWithAsync(null, image, removeImage: false, "testimonials", async avatarUrl =>
        {
            var testimonial = new Testimonial(dto.AuthorName, avatarUrl, dto.Rating, dto.Content, dto.AuthorBadge);
            db.Testimonials.Add(testimonial);
            await db.SaveChangesAsync();
            return testimonial.ToDto();
        });

    public async Task<bool> UpdateAsync(Guid id, TestimonialDto dto, IFormFile? image = null, bool removeImage = false)
    {
        var testimonial = await db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
        if (testimonial == null) return false;

        return await images.SaveWithAsync(testimonial.AuthorAvatarUrl, image, removeImage, "testimonials", async avatarUrl =>
        {
            testimonial.Update(dto.AuthorName, avatarUrl, dto.Rating, dto.Content, dto.AuthorBadge);
            await db.SaveChangesAsync();
            return true;
        });
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
