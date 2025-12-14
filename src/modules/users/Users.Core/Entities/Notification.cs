using BuildingBlocks.Domain.Entities;

namespace Users.Core.Entities;

/// <summary>
/// Notificação in-app do usuário.
/// Corresponde à tabela users.notifications no banco de dados.
/// </summary>
public class Notification : Entity
{
    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string NotificationType { get; private set; } = string.Empty;
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ActionUrl { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Relacionamento
    public User User { get; private set; } = null!;

    private Notification()
    {
    }

    public Notification(
        Guid userId,
        string title,
        string message,
        string notificationType,
        string? referenceType = null,
        Guid? referenceId = null,
        string? actionUrl = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        if (string.IsNullOrWhiteSpace(notificationType))
            throw new ArgumentException("Notification type cannot be empty.", nameof(notificationType));

        UserId = userId;
        Title = title;
        Message = message;
        NotificationType = notificationType;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        ActionUrl = actionUrl;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            ReadAt = DateTime.UtcNow;
        }
    }

    public void MarkAsUnread()
    {
        ReadAt = null;
    }

    public bool IsRead => ReadAt.HasValue;
}
