namespace Bcommerce.Modules.Payments.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Declined = 4,
    Failed = 5,
    Cancelled = 6,
    Refunded = 7,
    PartiallyRefunded = 8
}
