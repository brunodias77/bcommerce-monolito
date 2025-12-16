using Bcommerce.BuildingBlocks.Infrastructure.Data;
using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;

public class AuditLogRepository(BaseDbContext dbContext) : IAuditLogRepository
{
    private readonly BaseDbContext _dbContext = dbContext;

    public async Task AddAsync(AuditLogModel auditLog)
    {
        await _dbContext.Set<AuditLogModel>().AddAsync(auditLog);
    }
}
