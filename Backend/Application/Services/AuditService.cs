using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogChangeAsync(string entityType, Guid entityId, string action, Guid? userId, string? oldValues = null, string? newValues = null)
        {
            var auditLog = new AuditLog(
                Guid.NewGuid(),
                entityType,
                entityId,
                action,
                userId,
                oldValues,
                newValues
            );

            await _auditLogRepository.AddAsync(auditLog);
        }
    }
}

