namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record StockReservedEvent(Guid OrderId, Guid ProductId, int Quantity) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
