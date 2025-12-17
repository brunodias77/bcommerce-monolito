using AuditLogModel = Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Models.AuditLog;
using Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Repositories;

namespace Bcommerce.BuildingBlocks.Infrastructure.AuditLog.Services;

/// <summary>
/// Implementação padrão do serviço de auditoria.
/// </summary>
/// <remarks>
/// Delega a persistência para o repositório configurado.
/// - Ponto de extensão para regras de negócio de auditoria
/// - Mantém a responsabilidade de orquestração
/// 
/// Exemplo de uso:
/// <code>
/// // Injetado em interceptors ou handlers
/// _auditService.LogAsync(log);
/// </code>
/// </remarks>
public class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    private readonly IAuditLogRepository _repository = repository;

    /// <inheritdoc />
    public async Task LogAsync(AuditLogModel auditLog)
    {
        await _repository.AddAsync(auditLog);
    }
}
