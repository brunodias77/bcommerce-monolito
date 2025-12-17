using Bcommerce.BuildingBlocks.Infrastructure.Data;
using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;

/// <summary>
/// Implementação do repositório de auditoria com EF Core.
/// </summary>
/// <remarks>
/// Salva registros na tabela de auditoria via BaseDbContext.
/// - Implementa adição simples ao DbSet
/// - Não invoca SaveChanges (delegação de controle)
/// 
/// Exemplo de uso:
/// <code>
/// services.AddScoped&lt;IAuditLogRepository, AuditLogRepository&gt;();
/// </code>
/// </remarks>
public class AuditLogRepository(BaseDbContext dbContext) : IAuditLogRepository
{
    private readonly BaseDbContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task AddAsync(AuditLogModel auditLog)
    {
        await _dbContext.Set<AuditLogModel>().AddAsync(auditLog);
    }
}
