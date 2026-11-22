using Domain.Entities;
using Domain.Enums;
using FluentValidation;

namespace API.Features.Courses;

/// <summary>Enum fields (Type, Status, Modality) are exchanged as their identifiers, e.g. "EmBreve".</summary>
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Image { get; set; }
    public string Instructor { get; set; } = default!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Modality { get; set; }
    public decimal? Price { get; set; }
    public string Currency { get; set; } = "BRL";
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? Link { get; set; }
}

public static class CourseMappings
{
    public static CourseDto ToDto(this Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Image = course.Image,
        Instructor = course.Instructor,
        StartDate = course.StartDate,
        EndDate = course.EndDate,
        Modality = course.Modality?.ToString(),
        Price = course.Price?.Amount ?? 0,
        Currency = course.Price?.Currency ?? "BRL",
        Type = course.Type.ToString(),
        Status = course.Status.ToString(),
        Link = course.Link,
    };
}

public class CourseValidator : AbstractValidator<CourseDto>
{
    public CourseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Course.TitleMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(Course.DescriptionMaxLength);
        RuleFor(x => x.Instructor).NotEmpty().MaximumLength(Course.InstructorMaxLength);
        RuleFor(x => x.Link).MaximumLength(Course.LinkMaxLength);
        RuleFor(x => x.Type).IsEnumName(typeof(CourseType), caseSensitive: false);
        RuleFor(x => x.Status).IsEnumName(typeof(CourseStatus), caseSensitive: false);
        RuleFor(x => x.Modality).IsEnumName(typeof(Modality), caseSensitive: false).When(x => x.Modality != null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Currency).Length(3).When(x => x.Price.HasValue);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("A data de término não pode ser anterior à data de início.");
    }
}
