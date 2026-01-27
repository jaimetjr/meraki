using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyCollection<AuditLog>> GetByEntityTypeAsync(string entityType)
        {
            return (await _dbSet
                .Where(a => a.EntityType == entityType)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync()).AsReadOnly();
        }

        public async Task<IReadOnlyCollection<AuditLog>> GetByEntityIdAsync(Guid entityId)
        {
            return (await _dbSet
                .Where(a => a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync()).AsReadOnly();
        }

        public async Task<IReadOnlyCollection<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return (await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync()).AsReadOnly();
        }
    }
}

