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
        if (category == null) throw new ArgumentNullException(nameof(category));
        Category = category;
        // Only set CategoryId if the category already has an ID (existing entity)
        // For new categories, EF Core will set CategoryId automatically when SaveChanges is called
        if (category.Id != Guid.Empty)
        {
            CategoryId = category.Id;
        }
        MarkAsModified();
    }

    public void AddBenefit(Benefit benefit)
    {
        if (benefit == null) throw new ArgumentNullException(nameof(benefit));
        
        // Check for duplicates: by reference equality (works for both new and existing)
        // or by ID if the benefit already has an ID (existing entity)
        bool alreadyExists = Benefits.Contains(benefit) || 
                            (benefit.Id != Guid.Empty && Benefits.Any(b => b.Id == benefit.Id));
        
        if (!alreadyExists)
        {
            Benefits.Add(benefit);
            MarkAsModified();
        }
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
