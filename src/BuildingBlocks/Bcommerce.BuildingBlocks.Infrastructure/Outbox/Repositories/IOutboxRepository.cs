using Bcommerce.BuildingBlocks.Infrastructure.Outbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Outbox.Repositories;

/// <summary>
/// Contrato para persistência de mensagens na tabela Outbox.
/// </summary>
/// <remarks>
/// Abstração para adicionar mensagens à fila de saída.
/// - Usado internamente para enfileirar Domain Events
/// - Facilita testes e desacoplamento
/// 
/// Exemplo de uso:
/// <code>
/// await _outboxRepository.AddAsync(new OutboxMessage { ... });
/// </code>
/// </remarks>
public interface IOutboxRepository
{
    /// <summary>
    /// Adiciona uma mensagem à fila de Outbox.
    /// </summary>
    /// <param name="message">Mensagem a ser persistida.</param>
    Task AddAsync(OutboxMessage message);
}
