using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace Bcommerce.BuildingBlocks.Messaging.Abstractions;

// Reutiliza a interface do Application se possível, ou define nova.
// Como o Application já tem IIntegrationEvent, vamos usar.
// O IIntegrationEventPublisher do Messaging pode ser a implementação concreta ou uma interface que estende.
// Mas para seguir o pedido do usuário, vamos criar a interface aqui.

public interface IIntegrationEventPublisher
{
    Task Publish(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
