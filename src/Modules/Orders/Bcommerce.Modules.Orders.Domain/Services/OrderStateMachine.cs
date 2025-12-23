using Bcommerce.Modules.Orders.Domain.Entities;
using Bcommerce.Modules.Orders.Domain.Enums;

namespace Bcommerce.Modules.Orders.Domain.Services;

public class OrderStateMachine
{
    public bool CanTransitionTo(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.PendingPayment) => true,
            (OrderStatus.PendingPayment, OrderStatus.Paid) => true,
            (OrderStatus.PendingPayment, OrderStatus.Cancelled) => true,
            (OrderStatus.Paid, OrderStatus.Shipped) => true,
            (OrderStatus.Paid, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };
    }
}
