namespace Bcommerce.Modules.Orders.Domain.Enums;

public enum CancellationReason
{
    CustomerRequest = 1,
    OutOfStock = 2,
    PaymentFailed = 3,
    FraudSuspected = 4,
    Other = 5
}
