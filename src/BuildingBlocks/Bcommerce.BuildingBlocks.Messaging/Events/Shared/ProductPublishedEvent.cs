namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record ProductPublishedEvent(Guid ProductId, string Name, decimal Price, string Sku) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
