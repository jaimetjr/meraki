using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ServiceValidator : AbstractValidator<ServiceDto>
    {
        public ServiceValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
            RuleForEach(x => x.Benefits).SetValidator(new BenefitValidator());
            RuleFor(x => x.Category).NotNull().WithMessage("Category is required.");
            RuleFor(x => x.Category).SetValidator(new CategoryValidator()).When(x => x.Category != null);

        }
    }
}
