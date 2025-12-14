using BuildingBlocks.Domain.Entities;

namespace Users.Core.Entities;

/// <summary>
/// Preferências de notificação do usuário.
/// Corresponde à tabela users.notification_preferences no banco de dados.
/// </summary>
public class NotificationPreferences : Entity, IAuditableEntity
{
    public Guid UserId { get; private set; }

    public bool EmailEnabled { get; private set; } = true;
    public bool PushEnabled { get; private set; } = true;
    public bool SmsEnabled { get; private set; } = false;
    public bool OrderUpdates { get; private set; } = true;
    public bool Promotions { get; private set; } = true;
    public bool PriceDrops { get; private set; } = true;
    public bool BackInStock { get; private set; } = true;
    public bool Newsletter { get; private set; } = false;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Relacionamento
    public User User { get; private set; } = null!;

    private NotificationPreferences()
    {
    }

    public NotificationPreferences(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableEmail()
    {
        EmailEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableEmail()
    {
        EmailEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnablePush()
    {
        PushEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisablePush()
    {
        PushEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableSms()
    {
        SmsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableSms()
    {
        SmsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableOrderUpdates()
    {
        OrderUpdates = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableOrderUpdates()
    {
        OrderUpdates = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnablePromotions()
    {
        Promotions = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisablePromotions()
    {
        Promotions = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnablePriceDrops()
    {
        PriceDrops = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisablePriceDrops()
    {
        PriceDrops = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableBackInStock()
    {
        BackInStock = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableBackInStock()
    {
        BackInStock = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableNewsletter()
    {
        Newsletter = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableNewsletter()
    {
        Newsletter = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAll(
        bool emailEnabled,
        bool pushEnabled,
        bool smsEnabled,
        bool orderUpdates,
        bool promotions,
        bool priceDrops,
        bool backInStock,
        bool newsletter)
    {
        EmailEnabled = emailEnabled;
        PushEnabled = pushEnabled;
        SmsEnabled = smsEnabled;
        OrderUpdates = orderUpdates;
        Promotions = promotions;
        PriceDrops = priceDrops;
        BackInStock = backInStock;
        Newsletter = newsletter;
        UpdatedAt = DateTime.UtcNow;
    }
}
