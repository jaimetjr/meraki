using Domain.Entities;
using FluentValidation;

namespace API.Features.Contact;

/// <summary>What the public contact form sends.</summary>
public class ContactRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string? Message { get; set; }

    /// <summary>The visitor ticked "I agree that Meraki stores my data to answer me" (LGPD).</summary>
    public bool Consent { get; set; }

    /// <summary>Cloudflare Turnstile token produced by the widget in the visitor's browser.</summary>
    public string? CaptchaToken { get; set; }
}

public class ContactResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>A stored message as shown in the admin inbox.</summary>
public class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Service { get; set; } = default!;
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MarkReadDto
{
    public bool IsRead { get; set; }
}

public static class ContactMappings
{
    public static ContactMessageDto ToDto(this ContactMessage message) => new()
    {
        Id = message.Id,
        Name = message.Name,
        Email = message.Email,
        Phone = message.Phone,
        Service = message.ServiceName,
        Message = message.Message,
        IsRead = message.IsRead,
        CreatedAt = message.CreatedAt,
    };
}

public class ContactRequestValidator : AbstractValidator<ContactRequestDto>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ContactMessage.NameMaxLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(ContactMessage.EmailMaxLength);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(ContactMessage.PhoneMaxLength);
        RuleFor(x => x.Service).NotEmpty().MaximumLength(ContactMessage.ServiceNameMaxLength);
        RuleFor(x => x.Message).MaximumLength(ContactMessage.MessageMaxLength);
        RuleFor(x => x.Consent).Equal(true).WithMessage("É necessário autorizar o armazenamento dos seus dados de contato.");
    }
}
