using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class CourseValidator : AbstractValidator<CourseDto>
    {
        public CourseValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Instructor).NotEmpty();
            RuleFor(x => x.Type).IsEnumName(typeof(Domain.Enums.CourseType), caseSensitive: false);
            RuleFor(x => x.Status).IsEnumName(typeof(Domain.Enums.CourseStatus), caseSensitive: false);
            RuleFor(x => x.Modality).IsEnumName(typeof(Domain.Enums.Modality), caseSensitive: false).When(x => x.Modality != null);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
            RuleFor(x => x.Currency).Length(3).When(x => x.Price.HasValue);
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("EndDate must be greater than or equal to StartDate");
        }
    }

}
