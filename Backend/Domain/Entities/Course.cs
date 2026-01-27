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
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required", nameof(description));
            if (string.IsNullOrWhiteSpace(instructor)) throw new ArgumentException("Instructor is required", nameof(instructor));
            if (title.Length > 200) throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));
            if (description.Length > 1000) throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

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
                    throw new ArgumentException("EndDate cannot be before StartDate", nameof(endDate));
            }
            
            if (utcStartDate < DateTime.UtcNow) throw new ArgumentException("StartDate cannot be in the past", nameof(startDate));
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
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required", nameof(description));
            if (string.IsNullOrWhiteSpace(instructor)) throw new ArgumentException("Instructor is required", nameof(instructor));
            if (title.Length > 200) throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));
            if (description.Length > 1000) throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

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
