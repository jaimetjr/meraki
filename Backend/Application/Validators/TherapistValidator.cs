using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class TherapistValidator : AbstractValidator<TherapistDto>
    {
        public TherapistValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Bio).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Experience).NotEmpty();
            RuleFor(x => x.Education).NotEmpty();
            RuleForEach(x => x.Specialties).SetValidator(new SpecialtyValidator());
        }
    }

}
