using Bcommerce.Modules.Payments.Domain.Entities;
using Bcommerce.Modules.Payments.Domain.ValueObjects;
using Bcommerce.Modules.Payments.Infrastructure.Gateways.Abstractions;

namespace Bcommerce.Modules.Payments.Infrastructure.Gateways.MercadoPago;

public class MercadoPagoGateway : PaymentGatewayBase
{
    public override Task<PaymentTransaction> AuthorizeAsync(Payment payment, CardDetails cardDetails, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<PaymentTransaction> CaptureAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<PaymentTransaction> RefundAsync(Payment payment, decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override async Task<PixData> GeneratePixAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return new PixData("pix_qr_code_stub", "https://pix.mercadopago.com/qr/123", DateTime.UtcNow.AddMinutes(30));
    }

    public override async Task<BoletoData> GenerateBoletoAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return new BoletoData("123456789", "1234.5678.9101", "https://boleto.mercadopago.com/123", DateTime.UtcNow.AddDays(3));
    }
}
