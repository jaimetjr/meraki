using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IReadOnlyCollection<AuditLog>> GetByEntityTypeAsync(string entityType);
        Task<IReadOnlyCollection<AuditLog>> GetByEntityIdAsync(Guid entityId);
        Task<IReadOnlyCollection<AuditLog>> GetByUserIdAsync(Guid userId);
    }
}

