using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Benefit : AggregateRoot, ISoftDeletable
    {
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public List<Service> Services { get; private set; } = new();
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Benefit() { }

        public Benefit(string title, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (title.Length > 200) throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));
            if (description != null && description.Length > 1000) throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

            Title = title;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string title, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (title.Length > 200) throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));
            if (description != null && description.Length > 1000) throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

            Title = title;
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
