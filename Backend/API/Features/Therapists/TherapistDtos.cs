using API.Common;
using API.Features.Specialties;
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

    /// <summary>Sent as a JSON array in the "specialties" form field; only the ids are used.
    /// Omitted (null) on update keeps the current specialties.</summary>
    [ModelBinder(typeof(FormJsonModelBinder), Name = "specialties")]
    [ValidateNever]
    public List<SpecialtyDto>? Specialties { get; set; }
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
        Specialties = therapist.Specialties.Select(s => s.ToDto()).ToList(),
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
