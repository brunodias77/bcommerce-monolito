using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.ValueObjects;

namespace Bcommerce.Modules.Payments.Domain.Services;

public interface IPaymentGateway
{
    Task<PaymentTransaction> AuthorizeAsync(Payment payment, CardDetails cardDetails, CancellationToken cancellationToken = default);
    Task<PaymentTransaction> CaptureAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<PaymentTransaction> RefundAsync(Payment payment, decimal amount, CancellationToken cancellationToken = default);
    
    // Methods to generate Pix/Boleto would return data
    Task<PixData> GeneratePixAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<BoletoData> GenerateBoletoAsync(Payment payment, CancellationToken cancellationToken = default);
}
