using Domain.Exceptions;
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
        public const int TitleMaxLength = 100;
        public const int DescriptionMaxLength = 300;

        public string Title { get; private set; }
        public string? Description { get; private set; }
        public List<Service> Services { get; private set; } = new();
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Benefit() { }

        public Benefit(string title, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required");
            if (title.Length > TitleMaxLength) throw new DomainException($"Title cannot exceed {TitleMaxLength} characters");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

            Title = title;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string title, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required");
            if (title.Length > TitleMaxLength) throw new DomainException($"Title cannot exceed {TitleMaxLength} characters");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

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
