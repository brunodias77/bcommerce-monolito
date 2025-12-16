namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record StockReleasedEvent(Guid OrderId, Guid ProductId, int Quantity) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
