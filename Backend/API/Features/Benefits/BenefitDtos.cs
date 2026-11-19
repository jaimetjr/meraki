using Domain.Entities;
using FluentValidation;

namespace API.Features.Benefits;

public class BenefitDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}

public static class BenefitMappings
{
    public static BenefitDto ToDto(this Benefit benefit) =>
        new() { Id = benefit.Id, Title = benefit.Title, Description = benefit.Description };
}

public class BenefitValidator : AbstractValidator<BenefitDto>
{
    public BenefitValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Benefit.TitleMaxLength);
        RuleFor(x => x.Description).MaximumLength(Benefit.DescriptionMaxLength);
    }
}
