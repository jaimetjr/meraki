using API.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
    /// <summary>Null when the clinic doesn't publish a price ("sob consulta").</summary>
    public decimal? Price { get; set; }
    public string Currency { get; set; } = "BRL";

    /// <summary>JSON array of strings in the "benefits" form field, one per line the admin typed.
    /// Omitted (null) on update keeps the current benefits; [] clears them.</summary>
    [ModelBinder(typeof(FormJsonModelBinder), Name = "benefits")]
    [ValidateNever]
    public List<string>? Benefits { get; set; }

    /// <summary>Read-only: the therapists who offer this service (set from the therapist's form).</summary>
    [BindNever]
    [ValidateNever]
    public List<IdNameDto> Therapists { get; set; } = [];
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
        Price = service.Price?.Amount,
        Currency = service.Price?.Currency ?? "BRL",
        Benefits = [.. service.Benefits],
        Therapists = service.Therapists.OrderBy(t => t.Name).Select(t => new IdNameDto { Id = t.Id, Name = t.Name }).ToList(),
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
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Currency).NotEmpty().Length(3).When(x => x.Price.HasValue);
        // Same rules as Service.SetBenefits, which trims each line and drops empty ones.
        RuleFor(x => x.Benefits)
            .Must(b => b!.Count(line => !string.IsNullOrWhiteSpace(line)) <= Service.MaxBenefits)
            .When(x => x.Benefits != null)
            .WithMessage(Service.TooManyBenefitsMessage);
        RuleFor(x => x.Benefits)
            .Must(b => b!.All(line => (line?.Trim().Length ?? 0) <= Service.BenefitMaxLength))
            .When(x => x.Benefits != null)
            .WithMessage(Service.BenefitTooLongMessage);
    }
}
