using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class Notification : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    
    public string Title { get; private set; }
    public string Message { get; private set; }
    public string NotificationType { get; private set; } // INFO, WARNING, SUCCESS, ERROR
    
    public string? ReferenceType { get; private set; } // ORDER, PAYMENT, etc.
    public Guid? ReferenceId { get; private set; }
    
    public string? ActionUrl { get; private set; }
    
    public DateTime? ReadAt { get; private set; }

    protected Notification() { }

    public Notification(
        Guid userId,
        string title,
        string message,
        string notificationType,
        string? referenceType,
        Guid? referenceId,
        string? actionUrl)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Message = message;
        NotificationType = notificationType;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        ActionUrl = actionUrl;
    }

    public void MarkAsRead()
    {
        ReadAt = DateTime.UtcNow;
    }
}
