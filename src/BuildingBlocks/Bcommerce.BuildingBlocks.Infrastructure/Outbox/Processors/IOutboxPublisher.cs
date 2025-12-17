using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Processors;

/// <summary>
/// Interface para publicação de mensagens no Outbox (placeholder).
/// </summary>
/// <remarks>
/// Define contrato para persistência de eventos no Outbox.
/// - Geralmente usada por Interceptors ou Services de Aplicação
/// - Garante que o evento seja salvo na mesma transação do banco
/// 
/// Exemplo de uso:
/// <code>
/// // Implementação típica salvaria no DbSet&lt;OutboxMessage&gt;
/// await _outboxPublisher.Publish(evento);
/// </code>
/// </remarks>
public interface IOutboxPublisher
{
    // A implementação real usaria o IOutboxRepository dentro do escopo da transação
    // Mas o interceptor já tem acesso ao DbContext. 
    // Vamos criar apenas para formalizar se necessário, 
    // mas na prática o Interceptor fará o trabalho direto no DbContext.
}
