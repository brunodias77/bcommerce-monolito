using BuildingBlocks.Domain.Entities;

namespace Payments.Core.Entities;

public class PaymentWebhook : Entity
{
    public string GatewayName { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; } // JSONB
    public string? Headers { get; private set; } // JSONB
    
    public bool Processed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public Guid? PaymentId { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    protected PaymentWebhook() { }

    public PaymentWebhook(string gatewayName, string eventType, string payload, string? headers)
    {
        Id = Guid.NewGuid();
        GatewayName = gatewayName;
        EventType = eventType;
        Payload = payload;
        Headers = headers;
        ReceivedAt = DateTime.UtcNow;
        Processed = false;
    }

    public void MarkAsProcessed(Guid? paymentId)
    {
        Processed = true;
        ProcessedAt = DateTime.UtcNow;
        PaymentId = paymentId;
    }
    
    public void MarkAsFailed(string errorMessage)
    {
        Processed = false; // or true with error? usually processed=true means "we tried"
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}
