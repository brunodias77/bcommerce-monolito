using Bcommerce.BuildingBlocks.Domain.Base;
using Bcommerce.Modules.Payments.Domain.Enums;

namespace Bcommerce.Modules.Payments.Domain.Entities;

public class PaymentTransaction : Entity<Guid>
{
    public Guid PaymentId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public bool Success { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private PaymentTransaction() { }

    public PaymentTransaction(Guid paymentId, TransactionType type, decimal amount, bool success, string? gatewayTransactionId, string? errorMessage)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        Success = success;
        GatewayTransactionId = gatewayTransactionId;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
    }
}
