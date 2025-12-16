using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IIntegrationEventHandler<in TIntegrationEvent> : INotificationHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
}
