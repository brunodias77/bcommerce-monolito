using Bcommerce.BuildingBlocks.Application.Abstractions.Data;
using Bcommerce.Modules.Users.Domain.Entities;

namespace Bcommerce.Modules.Users.Domain.Repositories;

public interface ISessionRepository : IRepository<Session>
{
    Task<Session?> GetByRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
