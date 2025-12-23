using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Repositories;
using Bcommerce.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Repositories;

public class NotificationRepository : Repository<Notification, UsersDbContext>, INotificationRepository
{
    public NotificationRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .CountAsync(n => n.UserId == userId && n.ReadAt == null, cancellationToken);
    }
}
