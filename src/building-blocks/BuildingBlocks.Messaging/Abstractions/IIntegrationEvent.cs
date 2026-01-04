namespace BuildingBlocks.Messaging.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}