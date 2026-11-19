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
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (string.IsNullOrWhiteSpace(bio)) throw new DomainException("Bio is required");
            if (string.IsNullOrWhiteSpace(experience)) throw new DomainException("Experience is required");
            if (string.IsNullOrWhiteSpace(education)) throw new DomainException("Education is required");
            if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");
            if (bio.Length > BioMaxLength) throw new DomainException($"Bio cannot exceed {BioMaxLength} characters");

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

        public void AddSpecialty(Specialty specialty)
        {
            if (specialty == null) throw new ArgumentNullException(nameof(specialty));
            if (!Specialties.Any(x => x.Id == specialty.Id))
            {
                Specialties.Add(specialty);
                MarkAsModified();
            }
        }

        public void Update(string name, string bio, string? image, string experience, string education, string? proId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required");
            if (string.IsNullOrWhiteSpace(bio)) throw new DomainException("Bio is required");
            if (string.IsNullOrWhiteSpace(experience)) throw new DomainException("Experience is required");
            if (string.IsNullOrWhiteSpace(education)) throw new DomainException("Education is required");
            if (name.Length > NameMaxLength) throw new DomainException($"Name cannot exceed {NameMaxLength} characters");
            if (bio.Length > BioMaxLength) throw new DomainException($"Bio cannot exceed {BioMaxLength} characters");

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
