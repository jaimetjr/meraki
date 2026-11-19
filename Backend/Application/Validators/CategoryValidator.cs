using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class CategoryValidator : AbstractValidator<CategoryDto>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
    }
}
