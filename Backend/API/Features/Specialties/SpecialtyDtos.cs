using Domain.Entities;
using FluentValidation;

namespace API.Features.Specialties;

public class SpecialtyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public static class SpecialtyMappings
{
    public static SpecialtyDto ToDto(this Specialty specialty) =>
        new() { Id = specialty.Id, Name = specialty.Name, Description = specialty.Description };
}

public class SpecialtyValidator : AbstractValidator<SpecialtyDto>
{
    public SpecialtyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Specialty.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Specialty.DescriptionMaxLength);
    }
}
