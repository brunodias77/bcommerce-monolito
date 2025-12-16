namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record OrderPlacedEvent(Guid OrderId, Guid UserId, decimal TotalAmount) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
