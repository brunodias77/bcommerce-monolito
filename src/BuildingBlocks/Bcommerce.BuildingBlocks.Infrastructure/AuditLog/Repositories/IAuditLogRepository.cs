using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogModel auditLog);
}
