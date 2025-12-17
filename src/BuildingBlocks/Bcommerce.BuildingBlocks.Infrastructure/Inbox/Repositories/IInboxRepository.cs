using Bcommerce.BuildingBlocks.Infrastructure.Inbox.Models;

namespace Bcommerce.BuildingBlocks.Infrastructure.Inbox.Repositories;

/// <summary>
/// Contrato para persistência de mensagens no Inbox.
/// </summary>
/// <remarks>
/// Utilizado pelos Consumers para salvar eventos recebidos antes do processamento.
/// - Garante a persistência atômica da mensagem
/// - Abstrai o acesso a dados do Inbox
/// 
/// Exemplo de uso:
/// <code>
/// await _inboxRepository.AddAsync(new InboxMessage { ... });
/// </code>
/// </remarks>
public interface IInboxRepository
{
    /// <summary>
    /// Adiciona uma nova mensagem ao Inbox.
    /// </summary>
    /// <param name="message">A mensagem a ser persistida.</param>
    Task AddAsync(InboxMessage message);
}
