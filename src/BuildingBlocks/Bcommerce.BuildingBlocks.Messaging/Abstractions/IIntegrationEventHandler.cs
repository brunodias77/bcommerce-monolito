using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace Bcommerce.BuildingBlocks.Messaging.Abstractions;

// Interface genérica para handlers de eventos de integração
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken);
}

public interface IIntegrationEventHandler
{
}
