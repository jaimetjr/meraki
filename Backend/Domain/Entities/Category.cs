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
        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Category() { }

        public Category(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (name.Length > 100) throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

            Name = name;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
            if (name.Length > 100) throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

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
