using Domain.Entities;
using FluentValidation;

namespace API.Features.Categories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category category) => new() { Id = category.Id, Name = category.Name };
}

public class CategoryValidator : AbstractValidator<CategoryDto>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
    }
}
