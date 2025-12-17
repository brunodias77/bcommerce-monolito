using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Events;

public class UserRegisteredEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string UserName { get; }

    public UserRegisteredEvent(Guid userId, string email, string userName)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
    }
}
