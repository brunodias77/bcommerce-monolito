using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;

/// <summary>
/// Contrato para persistência de logs de auditoria.
/// </summary>
/// <remarks>
/// Define operações de escrita para o registro de auditoria.
/// - Inserção assíncrona de novos logs
/// - Desacopla a lógica de auditoria do acesso a dados
/// 
/// Exemplo de uso:
/// <code>
/// await _auditRepo.AddAsync(logs);
/// </code>
/// </remarks>
public interface IAuditLogRepository
{
    /// <summary>
    /// Persiste um registro de auditoria.
    /// </summary>
    /// <param name="auditLog">O log a ser salvo.</param>
    Task AddAsync(AuditLogModel auditLog);
}
