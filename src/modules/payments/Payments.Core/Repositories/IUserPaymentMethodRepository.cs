using BuildingBlocks.Domain.Repositories;
using Payments.Core.Entities;

namespace Payments.Core.Repositories;

public interface IUserPaymentMethodRepository : IRepository<UserPaymentMethod>
{
    Task<UserPaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<UserPaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserPaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(UserPaymentMethod method, CancellationToken cancellationToken);
    Task UpdateAsync(UserPaymentMethod method, CancellationToken cancellationToken);
    Task DeleteAsync(UserPaymentMethod method, CancellationToken cancellationToken);
}
