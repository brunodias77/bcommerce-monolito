using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
