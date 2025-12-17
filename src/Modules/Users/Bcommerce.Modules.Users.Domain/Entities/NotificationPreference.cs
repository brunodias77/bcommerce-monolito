using Bcommerce.BuildingBlocks.Domain.Base;

namespace Bcommerce.Modules.Users.Domain.Entities;

public class NotificationPreference : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    
    public bool EmailEnabled { get; private set; } = true;
    public bool PushEnabled { get; private set; } = true;
    public bool SmsEnabled { get; private set; } = false;
    
    // Categorias
    public bool OrderUpdates { get; private set; } = true;
    public bool Promotions { get; private set; } = true;
    public bool PriceDrops { get; private set; } = true;
    public bool BackInStock { get; private set; } = true;
    public bool Newsletter { get; private set; } = false;

    protected NotificationPreference() { }

    public NotificationPreference(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public void UpdateChannels(bool email, bool push, bool sms)
    {
        EmailEnabled = email;
        PushEnabled = push;
        SmsEnabled = sms;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategories(bool orderUpdates, bool promotions, bool priceDrops, bool backInStock, bool newsletter)
    {
        OrderUpdates = orderUpdates;
        Promotions = promotions;
        PriceDrops = priceDrops;
        BackInStock = backInStock;
        Newsletter = newsletter;
        UpdatedAt = DateTime.UtcNow;
    }
}
