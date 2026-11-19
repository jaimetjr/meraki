using Domain.Exceptions;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Category : AggregateRoot, ISoftDeletable
    {
        public const int NameMaxLength = 50;

        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Category() { }

        public Category(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");

            Name = name;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");

            Name = name;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }
}
