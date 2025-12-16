using Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

namespace Bcommerce.BuildingBlocks.Messaging.Events;

public abstract record IntegrationEvent(Guid EventId, DateTime OccurredOn) : IIntegrationEvent
{
    public string EventType => GetType().Name;
}
