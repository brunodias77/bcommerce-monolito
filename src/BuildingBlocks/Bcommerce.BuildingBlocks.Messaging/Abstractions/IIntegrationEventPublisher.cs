using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace Bcommerce.BuildingBlocks.Messaging.Abstractions;

// Reutiliza a interface do Application se possível, ou define nova.
// Como o Application já tem IIntegrationEvent, vamos usar.
// O IIntegrationEventPublisher do Messaging pode ser a implementação concreta ou uma interface que estende.
// Mas para seguir o pedido do usuário, vamos criar a interface aqui.

/// <summary>
/// Abstração para publicação explícita de eventos de integração.
/// </summary>
/// <remarks>
/// Especialização do publisher para tipos IIntegrationEvent.
/// - Garante que apenas eventos de integração sejam publicados por este canal
/// - Pode ser implementada sobre o IPublisher do MediatR ou MassTransit
/// 
/// Exemplo de uso:
/// <code>
/// await _integrationPublisher.Publish(new OrderCompletedEvent());
/// </code>
/// </remarks>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publica um evento de integração.
    /// </summary>
    /// <param name="integrationEvent">O evento a ser publicado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task Publish(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
