using API.Common;
using API.Features.Benefits;
using API.Features.Categories;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace API.Features.Services;

/// <summary>A service the clinic offers (therapy, session, …), not a code service.</summary>
public class ServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Image { get; set; }
    public string? LongDescription { get; set; }
    public string? Duration { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "BRL";

    /// <summary>JSON object in the "category" form field; only the id is used.</summary>
    [ModelBinder(typeof(FormJsonModelBinder), Name = "category")]
    public CategoryDto? Category { get; set; }

    /// <summary>JSON array in the "benefits" form field; only the ids are used.
    /// Omitted (null) on update keeps the current benefits.</summary>
    [ModelBinder(typeof(FormJsonModelBinder), Name = "benefits")]
    public List<BenefitDto>? Benefits { get; set; }
}

public static class ServiceMappings
{
    public static ServiceDto ToDto(this Service service) => new()
    {
        Id = service.Id,
        Name = service.Name,
        Description = service.Description,
        Image = service.Image,
        LongDescription = service.LongDescription,
        Duration = service.Duration,
        Price = service.Price.Amount,
        Currency = service.Price.Currency,
        Category = service.Category?.ToDto(),
        Benefits = service.Benefits.Select(b => b.ToDto()).ToList(),
    };
}

public class ServiceValidator : AbstractValidator<ServiceDto>
{
    public ServiceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Service.NameMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(Service.DescriptionMaxLength);
        RuleFor(x => x.LongDescription).MaximumLength(Service.LongDescriptionMaxLength);
        RuleFor(x => x.Duration).MaximumLength(Service.DurationMaxLength);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Category).NotNull().WithMessage("Category is required.");
    }
}
