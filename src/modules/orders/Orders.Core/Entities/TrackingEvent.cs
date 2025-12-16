using BuildingBlocks.Domain.Entities;

namespace Orders.Core.Entities;

public class TrackingEvent : Entity
{
    public Guid OrderId { get; private set; }
    public string EventCode { get; private set; }
    public string EventDescription { get; private set; }
    public string? Location { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected TrackingEvent() { }

    public TrackingEvent(Guid orderId, string eventCode, string eventDescription, string? location, string? city, string? state, DateTime occurredAt)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        EventCode = eventCode;
        EventDescription = eventDescription;
        Location = location;
        City = city;
        State = state;
        OccurredAt = occurredAt;
        CreatedAt = DateTime.UtcNow;
    }
}
