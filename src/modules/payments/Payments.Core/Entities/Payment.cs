using BuildingBlocks.Domain.Entities;
using Payments.Core.Enums;

namespace Payments.Core.Entities;

public class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public string IdempotencyKey { get; private set; }
    
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public decimal? FeeAmount { get; private set; }
    public decimal? NetAmount { get; private set; }
    
    public PaymentMethodType PaymentMethodType { get; private set; }
    public Guid? SavedPaymentMethodId { get; private set; }
    public string? PaymentMethodSnapshot { get; private set; } // JSONB
    
    public int Installments { get; private set; }
    public decimal? InstallmentAmount { get; private set; }
    
    // Gateway Info
    public string GatewayName { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayAuthorizationCode { get; private set; }
    public string? GatewayResponse { get; private set; } // JSONB
    
    // PIX
    public string? PixQrCode { get; private set; }
    public string? PixQrCodeUrl { get; private set; }
    public DateTime? PixExpirationAt { get; private set; }
    
    // Boleto
    public string? BoletoUrl { get; private set; }
    public string? BoletoBarcode { get; private set; }
    public DateTime? BoletoExpirationAt { get; private set; }
    
    public PaymentStatus Status { get; private set; }
    
    // Fraud
    public decimal? FraudScore { get; private set; }
    public string? FraudAnalysis { get; private set; } // JSONB
    
    // Errors
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    // Basic Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? AuthorizedAt { get; private set; }
    public DateTime? CapturedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Collections
    private readonly List<PaymentTransaction> _transactions = new();
    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();
    
    private readonly List<PaymentRefund> _refunds = new();
    public IReadOnlyCollection<PaymentRefund> Refunds => _refunds.AsReadOnly();
    
    private readonly List<Chargeback> _chargebacks = new();
    public IReadOnlyCollection<Chargeback> Chargebacks => _chargebacks.AsReadOnly();

    protected Payment() { }

    public Payment(Guid orderId, Guid userId, string idempotencyKey, decimal amount, PaymentMethodType paymentMethodType, string gatewayName)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        UserId = userId;
        IdempotencyKey = idempotencyKey;
        Amount = amount;
        Currency = "BRL";
        PaymentMethodType = paymentMethodType;
        GatewayName = gatewayName;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Installments = 1;
    }

    public void AddTransaction(PaymentTransaction transaction)
    {
        _transactions.Add(transaction);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Authorize(string gatewayTransactionId, DateTime authorizedAt)
    {
        Status = PaymentStatus.Authorized;
        GatewayTransactionId = gatewayTransactionId;
        AuthorizedAt = authorizedAt;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Capture(DateTime capturedAt)
    {
        Status = PaymentStatus.Captured;
        CapturedAt = capturedAt;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Fail(string errorCode, string errorMessage)
    {
        Status = PaymentStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        FailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetPixDetails(string qrCode, string qrCodeUrl, DateTime expiration)
    {
        PixQrCode = qrCode;
        PixQrCodeUrl = qrCodeUrl;
        PixExpirationAt = expiration;
        ExpiresAt = expiration;
    }
    
    public void SetBoletoDetails(string url, string barcode, DateTime expiration)
    {
        BoletoUrl = url;
        BoletoBarcode = barcode;
        BoletoExpirationAt = expiration;
        ExpiresAt = expiration;
    }
}
