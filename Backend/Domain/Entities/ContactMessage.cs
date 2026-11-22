using Domain.Exceptions;

namespace Domain.Entities;

/// <summary>
/// A message sent through the public contact form. Holds personal (possibly health) data, so it is
/// hard-deleted and its field values are never copied into the audit log.
/// </summary>
public sealed class ContactMessage : Entity, IHoldsPersonalData
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 255;
    public const int PhoneMaxLength = 30;
    public const int ServiceNameMaxLength = 100;
    public const int MessageMaxLength = 2000;

    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string ServiceName { get; private set; } = default!;
    public string? Message { get; private set; }
    public DateTime ConsentGivenAt { get; private set; }
    public bool IsRead { get; private set; }

    private ContactMessage() { }

    public ContactMessage(string name, string email, string phone, string serviceName, string? message, DateTime consentGivenAt)
    {
        Require(name, "Nome", NameMaxLength);
        Require(email, "E-mail", EmailMaxLength);
        Require(phone, "Telefone", PhoneMaxLength);
        Require(serviceName, "Serviço", ServiceNameMaxLength);
        if (message is { Length: > MessageMaxLength })
            throw new DomainException($"O campo Mensagem deve ter no máximo {MessageMaxLength} caracteres.");

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        ServiceName = serviceName;
        Message = string.IsNullOrWhiteSpace(message) ? null : message;
        ConsentGivenAt = consentGivenAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void MarkRead(bool isRead)
    {
        IsRead = isRead;
        MarkAsModified();
    }

    private static void Require(string value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"O campo {field} é obrigatório.");
        if (value.Length > maxLength) throw new DomainException($"O campo {field} deve ter no máximo {maxLength} caracteres.");
    }
}

/// <summary>Marks entities whose field values must never be copied into the audit log.</summary>
public interface IHoldsPersonalData;
