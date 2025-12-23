namespace Bcommerce.Modules.Payments.Domain.Enums;

public enum TransactionType
{
    Authorization = 1,
    Capture = 2,
    Refund = 3,
    Void = 4
}
