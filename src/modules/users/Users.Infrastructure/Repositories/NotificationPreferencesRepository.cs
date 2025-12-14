using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Core.Entities;
using Users.Core.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de preferências de notificação.
/// </summary>
internal class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly UsersDbContext _context;

    public NotificationPreferencesRepository(UsersDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<NotificationPreferences?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.Id == id, cancellationToken);
    }

    public async Task<NotificationPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default)
    {
        await _context.NotificationPreferences.AddAsync(preferences, cancellationToken);
    }

    public void Update(NotificationPreferences preferences)
    {
        _context.NotificationPreferences.Update(preferences);
    }
}