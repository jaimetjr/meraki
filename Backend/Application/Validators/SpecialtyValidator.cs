using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class SpecialtyValidator : AbstractValidator<SpecialtyDto>
    {
        public SpecialtyValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
        }
    }

}
