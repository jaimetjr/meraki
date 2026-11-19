using Domain.Exceptions;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public sealed class User : AggregateRoot, ISoftDeletable
    {
        public const int EmailMaxLength = 255;
        public const int NameMaxLength = 100;

        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core parameterless constructor
        private User() { }

        public User(Guid id, string email, string passwordHash, UserRole role)
        {
            if (id == Guid.Empty) throw new DomainException("Id cannot be empty");
            if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required");
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("PasswordHash is required");
            if (!IsValidEmail(email)) throw new DomainException("Invalid email format");

            Id = id;
            Email = email.ToLowerInvariant();
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email is required");
            if (!IsValidEmail(email))
                throw new DomainException("Invalid email format");

            Email = email.ToLowerInvariant();
            MarkAsModified();
        }

        public void UpdateProfile(string? firstName, string? lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            MarkAsModified();
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new DomainException("PasswordHash is required");

            PasswordHash = newPasswordHash;
            MarkAsModified();
        }

        public void UpdateRole(UserRole role)
        {
            Role = role;
            MarkAsModified();
        }

        public void Activate()
        {
            IsActive = true;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

