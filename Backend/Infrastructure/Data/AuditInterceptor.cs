using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Infrastructure.Data
{
    public class AuditInterceptor : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                await ProcessAuditEntriesAsync(eventData.Context, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task ProcessAuditEntriesAsync(DbContext context, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            var auditLogs = new List<AuditLog>();

            foreach (var entry in entries)
            {
                var entity = entry.Entity as Entity;
                if (entity == null) continue;

                // Skip AuditLog entities to avoid infinite recursion
                if (entity is AuditLog) continue;

                var entityType = entry.Entity.GetType().Name;
                var entityId = entity.Id;

                // Set CreatedBy/UpdatedBy on the entity
                if (entry.State == EntityState.Added)
                {
                    entity.SetCreatedBy(userId);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.SetUpdatedBy(userId);
                }

                // Create audit log entry
                string? oldValues = null;
                string? newValues = null;
                string action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => "Unknown"
                };

                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var originalValues = entry.OriginalValues.Properties
                        .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString() ?? "null");
                    oldValues = JsonSerializer.Serialize(originalValues);
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var currentValues = entry.CurrentValues.Properties
                        .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString() ?? "null");
                    newValues = JsonSerializer.Serialize(currentValues);
                }

                var auditLog = new AuditLog(
                    Guid.NewGuid(),
                    entityType,
                    entityId,
                    action,
                    userId,
                    oldValues,
                    newValues
                );

                auditLogs.Add(auditLog);
            }

            // Add all audit logs to the context
            if (auditLogs.Any())
            {
                context.Set<AuditLog>().AddRange(auditLogs);
            }
        }
    }
}

