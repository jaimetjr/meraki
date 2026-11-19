using Domain.Exceptions;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Specialty : AggregateRoot, ISoftDeletable
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 300;

        public string Name { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Specialty() { }

        public Specialty(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

            Name = name;
            Description = description;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }
}
