using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Repositories;
using Bcommerce.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Repositories;

public class AddressRepository : Repository<Address, UsersDbContext>, IAddressRepository
{
    public AddressRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);
    }
}
