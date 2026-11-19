using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Data;
using API.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Courses;

public class CourseService(AppDbContext db, ImageUploads images)
{
    public async Task<List<CourseDto>> GetAllAsync() =>
        (await db.Courses.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync()).ConvertAll(c => c.ToDto());

    public async Task<CourseDto?> GetByIdAsync(Guid id) =>
        (await db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id))?.ToDto();

    public async Task<List<CourseDto>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<CourseStatus>(status, ignoreCase: true, out var parsed))
            throw new DomainException($"Invalid course status: {status}");

        return (await db.Courses.AsNoTracking().Where(c => c.Status == parsed).ToListAsync()).ConvertAll(c => c.ToDto());
    }

    public async Task<CourseDto> CreateAsync(CourseDto dto, IFormFile? image = null)
    {
        var picture = await images.ApplyAsync(null, image, removeImage: false, "courses");

        var course = new Course(dto.Title, dto.Description, picture.Url, dto.Instructor,
            Enum.Parse<CourseType>(dto.Type, ignoreCase: true), Enum.Parse<CourseStatus>(dto.Status, ignoreCase: true));
        Schedule(course, dto);

        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, CourseDto dto, IFormFile? image = null, bool removeImage = false)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return false;

        var picture = await images.ApplyAsync(course.Image, image, removeImage, "courses");
        course.Update(dto.Title, dto.Description, picture.Url, dto.Instructor,
            Enum.Parse<CourseType>(dto.Type, ignoreCase: true), Enum.Parse<CourseStatus>(dto.Status, ignoreCase: true));
        Schedule(course, dto);

        await db.SaveChangesAsync();
        await images.CommitAsync(picture);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return false;

        course.Deactivate();
        await db.SaveChangesAsync();
        return true;
    }

    private static void Schedule(Course course, CourseDto dto)
    {
        if (!dto.StartDate.HasValue || string.IsNullOrEmpty(dto.Modality)) return;

        course.Schedule(dto.StartDate.Value, dto.EndDate, Enum.Parse<Modality>(dto.Modality, ignoreCase: true),
            new Money(dto.Price ?? 0, dto.Currency), dto.Link);
    }
}
