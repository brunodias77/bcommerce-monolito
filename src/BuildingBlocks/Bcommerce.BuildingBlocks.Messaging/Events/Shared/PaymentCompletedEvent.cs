namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record PaymentCompletedEvent(Guid PaymentId, Guid OrderId, decimal Amount) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
