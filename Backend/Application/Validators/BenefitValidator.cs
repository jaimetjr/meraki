using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class BenefitValidator : AbstractValidator<BenefitDto>
{
    public BenefitValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Benefit.TitleMaxLength);
        RuleFor(x => x.Description).MaximumLength(Benefit.DescriptionMaxLength);
    }
}
