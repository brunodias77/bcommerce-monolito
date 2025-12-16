using BuildingBlocks.Domain.Repositories;
using Payments.Core.Entities;

namespace Payments.Core.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken);
}
