using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public sealed class Service : AggregateRoot, ISoftDeletable
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 300;
    public const int LongDescriptionMaxLength = 1000;
    public const int DurationMaxLength = 50;

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? Image { get; private set; }
    public string? LongDescription { get; private set; }
    public List<Benefit> Benefits { get; private set; } = new();
    public string? Duration { get; private set; }
    public Money Price { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core parameterless constructor
    private Service() { }

    public Service(string name, string description, string? image, Money money)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required");
        if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");
        if (description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

        Name = name;
        Description = description;
        Image = string.IsNullOrWhiteSpace(image) ? null : image;
        Price = money ?? throw new ArgumentNullException(nameof(money));
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string? longDescription, string? duration)
    {
        if (longDescription != null && longDescription.Length > LongDescriptionMaxLength)
            throw new DomainException($"LongDescription cannot exceed {LongDescriptionMaxLength} characters");
        
        LongDescription = longDescription;
        Duration = duration;
        MarkAsModified();
    }

    public void UpdateCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        Category = category;
        CategoryId = category.Id;
        MarkAsModified();
    }

    /// <summary>Replaces the service's benefits with exactly the given set.</summary>
    public void SetBenefits(IEnumerable<Benefit> benefits)
    {
        ArgumentNullException.ThrowIfNull(benefits);
        Benefits.Clear();
        Benefits.AddRange(benefits.DistinctBy(b => b.Id));
        MarkAsModified();
    }

    public void Update(string name, string description, string? image, Money price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required");
        if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");
        if (description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

        Name = name;
        Description = description;
        Image = string.IsNullOrWhiteSpace(image) ? null : image;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        MarkAsModified();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsModified();
    }

}
