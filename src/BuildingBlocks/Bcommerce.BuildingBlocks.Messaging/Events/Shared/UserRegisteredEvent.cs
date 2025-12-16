namespace Bcommerce.BuildingBlocks.Messaging.Events.Shared;

public record UserRegisteredEvent(Guid UserId, string Email, string FirstName, string LastName) 
    : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow);
