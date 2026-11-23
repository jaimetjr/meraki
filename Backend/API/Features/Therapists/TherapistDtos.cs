using API.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace API.Features.Therapists;

public class TherapistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Bio { get; set; } = default!;
    public string? Image { get; set; }
    public string Experience { get; set; } = default!;
    public string Education { get; set; } = default!;
    public string? ProId { get; set; }

    /// <summary>The services this therapist offers. Sent as a JSON array in the "services" form field;
    /// only the ids are used. Omitted (null) on update keeps the current services; [] clears them.</summary>
    [ModelBinder(typeof(FormJsonModelBinder), Name = "services")]
    [ValidateNever]
    public List<IdNameDto>? Services { get; set; }
}

public static class TherapistMappings
{
    public static TherapistDto ToDto(this Therapist therapist) => new()
    {
        Id = therapist.Id,
        Name = therapist.Name,
        Bio = therapist.Bio,
        Image = therapist.Image,
        Experience = therapist.Experience,
        Education = therapist.Education,
        ProId = therapist.ProId,
        Services = therapist.Services.OrderBy(s => s.Name).Select(s => new IdNameDto { Id = s.Id, Name = s.Name }).ToList(),
    };
}

public class TherapistValidator : AbstractValidator<TherapistDto>
{
    public TherapistValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Therapist.NameMaxLength);
        RuleFor(x => x.Bio).NotEmpty().MaximumLength(Therapist.BioMaxLength);
        RuleFor(x => x.Experience).NotEmpty();
        RuleFor(x => x.Education).NotEmpty();
        RuleFor(x => x.ProId).MaximumLength(Therapist.ProIdMaxLength);
    }
}
