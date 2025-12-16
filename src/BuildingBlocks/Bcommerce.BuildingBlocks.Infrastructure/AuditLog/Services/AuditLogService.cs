using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;
using Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Services;

public class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    private readonly IAuditLogRepository _repository = repository;

    public async Task LogAsync(AuditLogModel auditLog)
    {
        await _repository.AddAsync(auditLog);
    }
}
