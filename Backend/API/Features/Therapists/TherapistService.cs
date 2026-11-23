using API.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Therapists;

public class TherapistService(AppDbContext db, ImageUploads images)
{
    private const string ServiceNotFound =
        "Um dos serviços selecionados não existe mais. Recarregue a página e tente novamente.";

    // The query filter hides deleted services from this list too.
    private IQueryable<Therapist> Therapists => db.Therapists.Include(t => t.Services);

    public async Task<List<TherapistDto>> GetAllAsync() =>
        (await Therapists.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync()).ConvertAll(t => t.ToDto());

    public async Task<TherapistDto?> GetByIdAsync(Guid id) =>
        (await Therapists.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id))?.ToDto();

    public Task<TherapistDto> CreateAsync(TherapistDto dto, IFormFile? image = null) =>
        images.SaveWithAsync(null, image, removeImage: false, "therapists", async imageUrl =>
        {
            var therapist = new Therapist(dto.Name, dto.Bio, imageUrl, dto.Experience, dto.Education, dto.ProId);
            therapist.SetServices(await db.Services.LoadAllByIdAsync(dto.Services?.Select(s => s.Id), ServiceNotFound));

            db.Therapists.Add(therapist);
            await db.SaveChangesAsync();
            return therapist.ToDto();
        });

    public async Task<bool> UpdateAsync(Guid id, TherapistDto dto, IFormFile? image = null, bool removeImage = false)
    {
        var therapist = await Therapists.FirstOrDefaultAsync(t => t.Id == id);
        if (therapist == null) return false;

        return await images.SaveWithAsync(therapist.Image, image, removeImage, "therapists", async imageUrl =>
        {
            therapist.Update(dto.Name, dto.Bio, imageUrl, dto.Experience, dto.Education, dto.ProId);
            if (dto.Services != null)
                therapist.SetServices(await db.Services.LoadAllByIdAsync(dto.Services.Select(s => s.Id), ServiceNotFound));

            await db.SaveChangesAsync();
            return true;
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var therapist = await db.Therapists.FirstOrDefaultAsync(t => t.Id == id);
        if (therapist == null) return false;

        therapist.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }
}
