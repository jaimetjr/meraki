using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class SpecialtyValidator : AbstractValidator<SpecialtyDto>
{
    public SpecialtyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Specialty.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Specialty.DescriptionMaxLength);
    }
}
