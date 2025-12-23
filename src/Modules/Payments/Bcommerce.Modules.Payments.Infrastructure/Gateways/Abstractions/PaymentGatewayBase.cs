using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.Services;
using Bcommerce.Modules.Payments.Domain.ValueObjects;

namespace Bcommerce.Modules.Payments.Infrastructure.Gateways.Abstractions;

public abstract class PaymentGatewayBase : IPaymentGateway
{
    public abstract Task<PaymentTransaction> AuthorizeAsync(Payment payment, CardDetails cardDetails, CancellationToken cancellationToken = default);
    public abstract Task<PaymentTransaction> CaptureAsync(Payment payment, CancellationToken cancellationToken = default);
    public abstract Task<PaymentTransaction> RefundAsync(Payment payment, decimal amount, CancellationToken cancellationToken = default);
    public abstract Task<PixData> GeneratePixAsync(Payment payment, CancellationToken cancellationToken = default);
    public abstract Task<BoletoData> GenerateBoletoAsync(Payment payment, CancellationToken cancellationToken = default);
}
