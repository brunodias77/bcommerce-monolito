using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Orders.Domain.ValueObjects;

namespace Bcommerce.Modules.Orders.Domain.Entities;

public class TrackingEvent : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public TrackingCode TrackingCode { get; private set; }
    public string Description { get; private set; }
    public string Location { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private TrackingEvent() { }

    public TrackingEvent(Guid orderId, TrackingCode trackingCode, string description, string location, DateTime occurredAt)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        TrackingCode = trackingCode;
        Description = description;
        Location = location;
        OccurredAt = occurredAt;
    }
}
