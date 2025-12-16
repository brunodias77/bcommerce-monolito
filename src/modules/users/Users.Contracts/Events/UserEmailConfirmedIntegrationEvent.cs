using BuildingBlocks.Domain.Events;

namespace Users.Contracts.Events;

public class UserEmailConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; }

    public UserEmailConfirmedIntegrationEvent(Guid userId, string email) : base("users")
    {
        UserId = userId;
        Email = email;
    }
}
