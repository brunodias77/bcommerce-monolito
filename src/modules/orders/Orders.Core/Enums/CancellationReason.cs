namespace Orders.Core.Enums;

public enum CancellationReason
{
    CustomerRequest,
    PaymentFailed,
    OutOfStock,
    FraudSuspected,
    ShippingIssue,
    Other
}
