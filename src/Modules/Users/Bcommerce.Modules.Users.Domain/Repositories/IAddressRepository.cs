using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Users.Domain.Entities;

namespace Bcommerce.Modules.Users.Domain.Repositories;

public interface IAddressRepository : IRepository<Address>
{
    Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);
}
