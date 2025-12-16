using BuildingBlocks.Domain.Entities;
using Payments.Core.Enums;

namespace Payments.Core.Entities;

public class PaymentTransaction : Entity
{
    public Guid PaymentId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayResponse { get; private set; } // JSONB
    public bool Success { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected PaymentTransaction() { }

    public PaymentTransaction(Guid paymentId, TransactionType transactionType, decimal amount, string? gatewayTransactionId, string? gatewayResponse, bool success, string? errorCode, string? errorMessage)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        TransactionType = transactionType;
        Amount = amount;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponse = gatewayResponse;
        Success = success;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CreatedAt = DateTime.UtcNow;
    }
}
