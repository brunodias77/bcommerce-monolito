namespace Payments.Core.Enums;

public enum PaymentStatus
{
    Pending,
    Processing,
    Authorized,
    Captured,
    Failed,
    Cancelled,
    Refunded,
    PartiallyRefunded,
    Chargeback,
    Expired
}
