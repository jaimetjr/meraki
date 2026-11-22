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
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("O campo Título é obrigatório.");
            if (title.Length > TitleMaxLength) throw new DomainException($"O campo Título deve ter no máximo {TitleMaxLength} caracteres.");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"O campo Descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

            Title = title;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string title, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("O campo Título é obrigatório.");
            if (title.Length > TitleMaxLength) throw new DomainException($"O campo Título deve ter no máximo {TitleMaxLength} caracteres.");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"O campo Descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

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
