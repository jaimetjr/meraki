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

        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Specialty() { }

        public Specialty(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O campo Nome é obrigatório.");
            if (name.Length > NameMaxLength) throw new DomainException($"O campo Nome deve ter no máximo {NameMaxLength} caracteres.");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"O campo Descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O campo Nome é obrigatório.");
            if (name.Length > NameMaxLength) throw new DomainException($"O campo Nome deve ter no máximo {NameMaxLength} caracteres.");
            if (description != null && description.Length > DescriptionMaxLength) throw new DomainException($"O campo Descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

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
