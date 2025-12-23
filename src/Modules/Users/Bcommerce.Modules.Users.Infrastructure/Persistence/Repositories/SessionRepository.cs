using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Repositories;
using Bcommerce.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Repositories;

public class SessionRepository : Repository<Session, UsersDbContext>, ISessionRepository
{
    public SessionRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<Session?> GetByRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return await DbContext.Sessions
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash, cancellationToken);
    }

    public async Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // This is a batch update, but EF Core 7+ supports it nicely.
        // Or we can fetch and iterate. Let's use ExecuteUpdate if possible, but Repository is generic.
        // Since we have specific method here, we can use ExecuteUpdate.
        
        await DbContext.Sessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RevokedAt, DateTime.UtcNow)
                .SetProperty(x => x.RevokedReason, "Revoked by system or logout all"), cancellationToken);
    }
}
