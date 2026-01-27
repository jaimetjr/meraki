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
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Specialty() { }

        public Specialty(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (name.Length > 100) throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));
            if (description != null && description.Length > 500) throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (name.Length > 100) throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));
            if (description != null && description.Length > 500) throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

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
