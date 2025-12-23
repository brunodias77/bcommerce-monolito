using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Payments.Domain.Entities;

namespace Bcommerce.Modules.Payments.Domain.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
