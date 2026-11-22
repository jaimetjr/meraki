using Domain.Exceptions;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Therapist : AggregateRoot, ISoftDeletable
    {
        public const int NameMaxLength = 100;
        public const int BioMaxLength = 500;
        public const int ProIdMaxLength = 200;

        public string Name { get; private set; }
        public string Bio { get; private set; }
        public string? Image { get; private set; }
        public string Experience { get; private set; }
        public string Education { get; private set; }
        public string? ProId { get; private set; }
        public List<Specialty> Specialties { get; private set; } = new();
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Therapist() { }

        public Therapist(string name, string bio, string? image, string experience, string education, string? proId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O campo Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(bio)) throw new DomainException("O campo Bio é obrigatório.");
            if (string.IsNullOrWhiteSpace(experience)) throw new DomainException("O campo Experiência é obrigatório.");
            if (string.IsNullOrWhiteSpace(education)) throw new DomainException("O campo Formação é obrigatório.");
            if (name.Length > NameMaxLength) throw new DomainException($"O campo Nome deve ter no máximo {NameMaxLength} caracteres.");
            if (bio.Length > BioMaxLength) throw new DomainException($"O campo Bio deve ter no máximo {BioMaxLength} caracteres.");

            Name = name;
            Bio = bio;
            Image = string.IsNullOrWhiteSpace(image) ? null : image;
            Experience = experience;
            Education = education;
            ProId = string.IsNullOrWhiteSpace(proId) ? null : proId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>Replaces the therapist's specialties with exactly the given set.</summary>
        public void SetSpecialties(IEnumerable<Specialty> specialties)
        {
            ArgumentNullException.ThrowIfNull(specialties);
            Specialties.Clear();
            Specialties.AddRange(specialties.DistinctBy(s => s.Id));
            MarkAsModified();
        }

        public void Update(string name, string bio, string? image, string experience, string education, string? proId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O campo Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(bio)) throw new DomainException("O campo Bio é obrigatório.");
            if (string.IsNullOrWhiteSpace(experience)) throw new DomainException("O campo Experiência é obrigatório.");
            if (string.IsNullOrWhiteSpace(education)) throw new DomainException("O campo Formação é obrigatório.");
            if (name.Length > NameMaxLength) throw new DomainException($"O campo Nome deve ter no máximo {NameMaxLength} caracteres.");
            if (bio.Length > BioMaxLength) throw new DomainException($"O campo Bio deve ter no máximo {BioMaxLength} caracteres.");

            Name = name;
            Bio = bio;
            Image = string.IsNullOrWhiteSpace(image) ? null : image;
            Experience = experience;
            Education = education;
            ProId = string.IsNullOrWhiteSpace(proId) ? null : proId;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }
}
