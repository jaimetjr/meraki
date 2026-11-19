using API.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Therapists;

public class TherapistService(AppDbContext db, ImageUploads images)
{
    private IQueryable<Therapist> Therapists => db.Therapists.Include(t => t.Specialties);

    public async Task<List<TherapistDto>> GetAllAsync() =>
        (await Therapists.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync()).ConvertAll(t => t.ToDto());

    public async Task<TherapistDto?> GetByIdAsync(Guid id) =>
        (await Therapists.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id))?.ToDto();

    public async Task<List<TherapistDto>> FindBySpecialtyAsync(string specialtyName) =>
        (await Therapists.AsNoTracking()
            .Where(t => t.Specialties.Any(s => s.Name == specialtyName))
            .ToListAsync()).ConvertAll(t => t.ToDto());

    public async Task<TherapistDto> CreateAsync(TherapistDto dto, IFormFile? image = null)
    {
        var picture = await images.ApplyAsync(null, image, removeImage: false, "therapists");

        var therapist = new Therapist(dto.Name, dto.Bio, picture.Url, dto.Experience, dto.Education, dto.ProId);
        therapist.SetSpecialties(await db.Specialties.LoadAllByIdAsync(dto.Specialties?.Select(s => s.Id), "specialties"));

        db.Therapists.Add(therapist);
        await db.SaveChangesAsync();
        return therapist.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, TherapistDto dto, IFormFile? image = null, bool removeImage = false)
    {
        var therapist = await Therapists.FirstOrDefaultAsync(t => t.Id == id);
        if (therapist == null) return false;

        var picture = await images.ApplyAsync(therapist.Image, image, removeImage, "therapists");
        therapist.Update(dto.Name, dto.Bio, picture.Url, dto.Experience, dto.Education, dto.ProId);
        if (dto.Specialties != null)
            therapist.SetSpecialties(await db.Specialties.LoadAllByIdAsync(dto.Specialties.Select(s => s.Id), "specialties"));

        await db.SaveChangesAsync();
        await images.CommitAsync(picture);
        return true;
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
