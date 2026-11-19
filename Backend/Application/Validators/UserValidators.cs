using Application.DTOs;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public const int PasswordMinLength = 8;

    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(User.EmailMaxLength);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(PasswordMinLength);
        RuleFor(x => x.FirstName).MaximumLength(User.NameMaxLength);
        RuleFor(x => x.LastName).MaximumLength(User.NameMaxLength);
        RuleFor(x => x.Role).IsInEnum();
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(User.EmailMaxLength);
        RuleFor(x => x.FirstName).MaximumLength(User.NameMaxLength);
        RuleFor(x => x.LastName).MaximumLength(User.NameMaxLength);
        RuleFor(x => x.Role).IsInEnum().When(x => x.Role.HasValue);
    }
}

public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordDto>
{
    public UpdatePasswordValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MinimumLength(CreateUserValidator.PasswordMinLength);
    }
}
