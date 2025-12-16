namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record OrderStatusChangedEvent(Guid OrderId, string NewStatus, string OldStatus) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
