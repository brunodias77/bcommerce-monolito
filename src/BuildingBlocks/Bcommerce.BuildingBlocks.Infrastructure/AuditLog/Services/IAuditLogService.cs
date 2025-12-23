using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Services;

/// <summary>
/// Serviço de aplicação para registro de auditoria.
/// </summary>
/// <remarks>
/// Facada para simplificar a criação de logs de auditoria.
/// - Encapsula o repositório
/// - Pode conter lógica adicional de enriquecimento de logs
/// 
/// Exemplo de uso:
/// <code>
/// await _auditService.LogAsync(new AuditLog { ... });
/// </code>
/// </remarks>
public interface IAuditLogService
{
    /// <summary>
    /// Registra um evento de auditoria.
    /// </summary>
    /// <param name="auditLog">Objeto de log.</param>
    Task LogAsync(AuditLogModel auditLog);
}
