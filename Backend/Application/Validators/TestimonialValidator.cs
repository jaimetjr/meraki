using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class TestimonialValidator : AbstractValidator<TestimonialDto>
{
    public TestimonialValidator()
    {
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(Testimonial.AuthorNameMaxLength);
        RuleFor(x => x.AuthorAvatarUrl).MaximumLength(Testimonial.AuthorAvatarUrlMaxLength);
        RuleFor(x => x.AuthorBadge).MaximumLength(Testimonial.AuthorBadgeMaxLength);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(Testimonial.ContentMaxLength);
    }
}
