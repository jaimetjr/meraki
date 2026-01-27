using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class BenefitValidator : AbstractValidator<BenefitDto>
    {
        public BenefitValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
        }
    }

}
