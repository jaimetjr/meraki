using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class TherapistValidator : AbstractValidator<TherapistDto>
{
    public TherapistValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Therapist.NameMaxLength);
        RuleFor(x => x.Bio).NotEmpty().MaximumLength(Therapist.BioMaxLength);
        RuleFor(x => x.Experience).NotEmpty();
        RuleFor(x => x.Education).NotEmpty();
        RuleFor(x => x.ProId).MaximumLength(Therapist.ProIdMaxLength);
        RuleForEach(x => x.Specialties).SetValidator(new SpecialtyValidator());
    }
}
