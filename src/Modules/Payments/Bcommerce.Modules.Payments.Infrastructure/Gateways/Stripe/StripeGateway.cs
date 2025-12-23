using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.Enums;
using Bcommerce.Modules.Payments.Domain.ValueObjects;
using Bcommerce.Modules.Payments.Infrastructure.Gateways.Abstractions;

namespace Bcommerce.Modules.Payments.Infrastructure.Gateways.Stripe;

public class StripeGateway : PaymentGatewayBase
{
    public override async Task<PaymentTransaction> AuthorizeAsync(Payment payment, CardDetails cardDetails, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        // Stub success
        return new PaymentTransaction(payment.Id, TransactionType.Authorization, payment.Amount.Value, true, "stripe_auth_123", null);
    }

    public override async Task<PaymentTransaction> CaptureAsync(Payment payment, CancellationToken cancellationToken = default)
    {
         await Task.Delay(100, cancellationToken);
        return new PaymentTransaction(payment.Id, TransactionType.Capture, payment.Amount.Value, true, "stripe_cap_123", null);
    }

    public override async Task<PaymentTransaction> RefundAsync(Payment payment, decimal amount, CancellationToken cancellationToken = default)
    {
         await Task.Delay(100, cancellationToken);
        return new PaymentTransaction(payment.Id, TransactionType.Refund, amount, true, "stripe_ref_123", null);
    }

    public override Task<PixData> GeneratePixAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Stripe Pix Stub not implemented");
    }

    public override Task<BoletoData> GenerateBoletoAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Stripe Boleto Stub not implemented");
    }
}
