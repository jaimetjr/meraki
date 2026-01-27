using System;

namespace Domain.Entities
{
    public sealed class AuditLog : Entity
    {
        public string EntityType { get; private set; }
        public Guid EntityId { get; private set; }
        public string Action { get; private set; } // Create, Update, Delete
        public Guid? UserId { get; private set; }
        public string? OldValues { get; private set; } // JSON serialized
        public string? NewValues { get; private set; } // JSON serialized
        public DateTime Timestamp { get; private set; }

        // EF Core parameterless constructor
        private AuditLog() { }

        public AuditLog(Guid id, string entityType, Guid entityId, string action, Guid? userId, string? oldValues = null, string? newValues = null)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty", nameof(id));
            if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("EntityType is required", nameof(entityType));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required", nameof(action));

            Id = id;
            EntityType = entityType;
            EntityId = entityId;
            Action = action;
            UserId = userId;
            OldValues = oldValues;
            NewValues = newValues;
            Timestamp = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

