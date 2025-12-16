namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record PaymentFailedEvent(Guid PaymentId, Guid OrderId, string Reason) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
