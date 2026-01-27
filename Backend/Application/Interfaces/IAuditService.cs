using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAuditService
    {
        Task LogChangeAsync(string entityType, Guid entityId, string action, Guid? userId, string? oldValues = null, string? newValues = null);
    }
}

