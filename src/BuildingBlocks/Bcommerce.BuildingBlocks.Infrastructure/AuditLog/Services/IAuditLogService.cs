using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Services;

public interface IAuditLogService
{
    Task LogAsync(AuditLogModel auditLog);
}
