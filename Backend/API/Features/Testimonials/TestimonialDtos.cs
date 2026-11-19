using Domain.Entities;
using FluentValidation;

namespace API.Features.Testimonials;

public class TestimonialDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = default!;
    public string? AuthorAvatarUrl { get; set; }
    public string? AuthorBadge { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = default!;
}

public static class TestimonialMappings
{
    public static TestimonialDto ToDto(this Testimonial testimonial) => new()
    {
        Id = testimonial.Id,
        AuthorName = testimonial.AuthorName,
        AuthorAvatarUrl = testimonial.AuthorAvatarUrl,
        AuthorBadge = testimonial.AuthorBadge,
        Rating = testimonial.Rating,
        Content = testimonial.Content,
    };
}

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
