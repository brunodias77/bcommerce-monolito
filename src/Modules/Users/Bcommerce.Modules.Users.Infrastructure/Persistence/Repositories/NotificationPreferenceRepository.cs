using Bcommerce.BuildingBlocks.Infrastructure.Data;
using Bcommerce.Modules.Users.Domain.Entities;
using Bcommerce.Modules.Users.Domain.Repositories;
using Bcommerce.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bcommerce.Modules.Users.Infrastructure.Persistence.Repositories;

public class NotificationPreferenceRepository : Repository<NotificationPreference, UsersDbContext>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(UsersDbContext context) : base(context)
    {
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId, cancellationToken);
    }
}
