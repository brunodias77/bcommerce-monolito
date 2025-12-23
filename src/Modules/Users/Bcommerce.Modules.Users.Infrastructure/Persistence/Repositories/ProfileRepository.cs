using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Repositories;
using Bcommerce.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Repositories;

public class ProfileRepository : Repository<Profile, UsersDbContext>, IProfileRepository
{
    public ProfileRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<Profile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}
