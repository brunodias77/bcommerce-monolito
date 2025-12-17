using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Users.Domain.Entities;

namespace Bcommerce.Modules.Users.Domain.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
