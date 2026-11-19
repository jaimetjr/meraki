using Domain.Entities;
using Domain.Enums;
using FluentValidation;

namespace API.Features.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.Admin;
}

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdatePasswordDto
{
    public string Password { get; set; } = string.Empty;
}

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        IsActive = user.IsActive,
    };
}

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
