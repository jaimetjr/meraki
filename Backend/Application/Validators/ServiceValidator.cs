using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

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
        RuleForEach(x => x.Benefits).SetValidator(new BenefitValidator());
        RuleFor(x => x.Category).NotNull().WithMessage("Category is required.");
        RuleFor(x => x.Category!).SetValidator(new CategoryValidator()).When(x => x.Category != null);
    }
}
