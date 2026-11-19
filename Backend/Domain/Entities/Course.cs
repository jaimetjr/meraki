using Domain.Exceptions;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Course : AggregateRoot, ISoftDeletable
    {
        public const int TitleMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int InstructorMaxLength = 100;
        public const int LinkMaxLength = 300;

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string? Image { get; private set; }
        public string Instructor { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public Modality? Modality { get; private set; }
        public Money? Price { get; private set; }
        public CourseType Type { get; private set; }
        public CourseStatus Status { get; private set; }
        public string? Link { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private Course() { }

        public Course(string title, string description, string? image, string instructor, CourseType type, CourseStatus status)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required");
            if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required");
            if (string.IsNullOrWhiteSpace(instructor)) throw new DomainException("Instructor is required");
            if (title.Length > TitleMaxLength) throw new DomainException($"Title cannot exceed {TitleMaxLength} characters");
            if (description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

            Title = title;
            Description = description;
            Image = string.IsNullOrWhiteSpace(image) ? null : image;
            Instructor = instructor;
            Type = type;
            Status = status;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Schedule(DateTime startDate, DateTime? endDate, Modality modality, Money price, string? link)
        {
            // Ensure DateTime is UTC for PostgreSQL compatibility
            DateTime utcStartDate;
            if (startDate.Kind == DateTimeKind.Utc)
            {
                utcStartDate = startDate;
            }
            else if (startDate.Kind == DateTimeKind.Unspecified)
            {
                // Assume Unspecified dates from API are already in UTC and just specify the kind
                utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            }
            else
            {
                // Convert Local to UTC
                utcStartDate = startDate.ToUniversalTime();
            }

            DateTime? utcEndDate = null;
            if (endDate.HasValue)
            {
                if (endDate.Value.Kind == DateTimeKind.Utc)
                {
                    utcEndDate = endDate.Value;
                }
                else if (endDate.Value.Kind == DateTimeKind.Unspecified)
                {
                    utcEndDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                }
                else
                {
                    utcEndDate = endDate.Value.ToUniversalTime();
                }

                if (utcEndDate < utcStartDate)
                    throw new DomainException("EndDate cannot be before StartDate");
            }
            
            // A past start date is only rejected when it is being set or changed, so courses that
            // already started remain editable.
            if (utcStartDate != StartDate && utcStartDate < DateTime.UtcNow)
                throw new DomainException("StartDate cannot be in the past");
            if (price == null) throw new ArgumentNullException(nameof(price));

            StartDate = utcStartDate;
            EndDate = utcEndDate;
            Modality = modality;
            Price = price;
            Link = link;
            MarkAsModified();
        }

        public void Update(string title, string description, string? image, string instructor, CourseType type, CourseStatus status)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required");
            if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required");
            if (string.IsNullOrWhiteSpace(instructor)) throw new DomainException("Instructor is required");
            if (title.Length > TitleMaxLength) throw new DomainException($"Title cannot exceed {TitleMaxLength} characters");
            if (description.Length > DescriptionMaxLength) throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters");

            Title = title;
            Description = description;
            Image = string.IsNullOrWhiteSpace(image) ? null : image;
            Instructor = instructor;
            Type = type;
            Status = status;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }
}
