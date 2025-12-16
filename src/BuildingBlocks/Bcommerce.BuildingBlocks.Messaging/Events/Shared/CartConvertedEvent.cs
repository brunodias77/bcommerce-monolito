namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record CartConvertedEvent(Guid CartId, Guid OrderId, Guid UserId) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
