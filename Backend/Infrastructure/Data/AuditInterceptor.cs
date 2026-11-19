using System.Text.Json;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Data;

/// <summary>
/// Stamps CreatedBy/UpdatedBy with the current user and writes one AuditLog row (old/new values
/// as JSON) for every added, modified or deleted <see cref="Entity"/>.
/// </summary>
public class AuditInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null) AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null) AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext context)
    {
        var userId = currentUser.UserId;
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is Entity and not AuditLog
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var auditLogs = new List<AuditLog>();
        foreach (var entry in entries)
        {
            var entity = (Entity)entry.Entity;

            if (entry.State == EntityState.Added) entity.SetCreatedBy(userId);
            else if (entry.State == EntityState.Modified) entity.SetUpdatedBy(userId);

            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                _ => "Delete",
            };

            string? oldValues = null;
            string? newValues = null;
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                oldValues = JsonSerializer.Serialize(entry.OriginalValues.Properties
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString() ?? "null"));
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                newValues = JsonSerializer.Serialize(entry.CurrentValues.Properties
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString() ?? "null"));
            }

            auditLogs.Add(new AuditLog(Guid.NewGuid(), entry.Entity.GetType().Name, entity.Id, action, userId, oldValues, newValues));
        }

        if (auditLogs.Count > 0) context.Set<AuditLog>().AddRange(auditLogs);
    }
}
