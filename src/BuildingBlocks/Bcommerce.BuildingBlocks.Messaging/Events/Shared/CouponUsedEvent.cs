namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record CouponUsedEvent(Guid CouponId, Guid OrderId, Guid UserId) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
