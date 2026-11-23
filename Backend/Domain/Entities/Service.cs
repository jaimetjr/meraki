using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Service : AggregateRoot, ISoftDeletable
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 300;
    public const int LongDescriptionMaxLength = 1000;
    public const int DurationMaxLength = 50;
    public const int BenefitMaxLength = 100;
    public const int MaxBenefits = 20;

    public static readonly string BenefitTooLongMessage = $"Cada benefício deve ter no máximo {BenefitMaxLength} caracteres.";
    public static readonly string TooManyBenefitsMessage = $"Um serviço pode ter no máximo {MaxBenefits} benefícios.";

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? Image { get; private set; }
    public string? LongDescription { get; private set; }
    /// <summary>Short sentences shown as a list on the site, in the order the admin typed them.</summary>
    public List<string> Benefits { get; private set; } = new();
    public string? Duration { get; private set; }
    /// <summary>Null when the clinic doesn't publish a price ("sob consulta").</summary>
    public Money? Price { get; private set; }
    /// <summary>Who offers this service; managed from the therapist's side (<see cref="Therapist.SetServices"/>).</summary>
    public List<Therapist> Therapists { get; private set; } = new();
    public bool IsActive { get; private set; }

    // EF Core parameterless constructor
    private Service() { }

    public Service(string name, string description, string? image, Money? price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O campo Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("O campo Descrição é obrigatório.");
        if (name.Length > NameMaxLength) throw new DomainException($"O campo Nome deve ter no máximo {NameMaxLength} caracteres.");
        if (description.Length > DescriptionMaxLength) throw new DomainException($"O campo Descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        Name = name;
        Description = description;
        Image = string.IsNullOrWhiteSpace(image) ? null : image;
        Price = price;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string? longDescription, string? duration)
    {
        if (longDescription != null && longDescription.Length > LongDescriptionMaxLength)
            throw new DomainException($"O campo Descrição longa deve ter no máximo {LongDescriptionMaxLength} caracteres.");
        if (duration != null && duration.Length > DurationMaxLength)
            throw new DomainException($"O campo Duração deve ter no máximo {DurationMaxLength} caracteres.");

        LongDescription = string.IsNullOrWhiteSpace(longDescription) ? null : longDescription;
        Duration = string.IsNullOrWhiteSpace(duration) ? null : duration.Trim();
        MarkAsModified();
    }

    /// <summary>
    /// Replaces the benefits with exactly these lines: each is trimmed, and empty lines and repeats are dropped.
    /// </summary>
    public void SetBenefits(IEnumerable<string> benefits)
    {
        ArgumentNullException.ThrowIfNull(benefits);
        var lines = benefits
            .Select(b => b?.Trim())
            .Where(b => !string.IsNullOrEmpty(b))
            .Select(b => b!)
            .Distinct()
            .ToList();
        if (lines.Any(b => b.Length > BenefitMaxLength)) throw new DomainException(BenefitTooLongMessage);
        if (lines.Count > MaxBenefits) throw new DomainException(TooManyBenefitsMessage);

        Benefits = lines;
        MarkAsModified();
    }

    public void Update(string name, string description, string? image, Money? price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O campo Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("O campo Descrição é obrigatório.");
        if (name.Length > NameMaxLength) throw new DomainException($"O campo Nome deve ter no máximo {NameMaxLength} caracteres.");
        if (description.Length > DescriptionMaxLength) throw new DomainException($"O campo Descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        Name = name;
        Description = description;
        Image = string.IsNullOrWhiteSpace(image) ? null : image;
        Price = price;
        MarkAsModified();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsModified();
    }
}
