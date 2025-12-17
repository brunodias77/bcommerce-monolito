namespace Bcommerce.Modules.Orders.Domain.Enums;

public enum OrderStatus
{
    Draft = 1,
    PendingPayment = 2,
    Paid = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6
}
