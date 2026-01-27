using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class TestimonialValidator : AbstractValidator<TestimonialDto>
    {
        public TestimonialValidator()
        {
            RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AuthorAvatarUrl).MaximumLength(300);
            RuleFor(x => x.AuthorBadge).MaximumLength(50).When(x => x.AuthorBadge != null);
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Content).NotEmpty().MaximumLength(1000);
        }
    }

}
