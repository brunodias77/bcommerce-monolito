namespace Payments.Core.Enums;

public enum TransactionType
{
    Authorization,
    Capture,
    Void,
    Refund,
    Chargeback
}
