namespace Orders.Core.Enums;

public enum OrderStatus
{
    Pending,
    PaymentProcessing,
    Paid,
    Preparing,
    Shipped,
    OutForDelivery,
    Delivered,
    Cancelled,
    Refunded,
    Failed
}
