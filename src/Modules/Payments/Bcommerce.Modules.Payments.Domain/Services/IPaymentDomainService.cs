using Bcommerce.Modules.Payments.Domain.Entities;

namespace Bcommerce.Modules.Payments.Domain.Services;

public interface IPaymentDomainService
{
    Task ProcessPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
}
