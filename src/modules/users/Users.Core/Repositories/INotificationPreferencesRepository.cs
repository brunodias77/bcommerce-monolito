using BuildingBlocks.Domain.Repositories;
using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de preferências de notificação.
/// </summary>
public interface INotificationPreferencesRepository : IRepository<NotificationPreferences>
{
    /// <summary>
    /// Busca preferências de notificação por ID.
    /// </summary>
    Task<NotificationPreferences?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca preferências de notificação por user ID.
    /// </summary>
    Task<NotificationPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona novas preferências de notificação.
    /// </summary>
    Task AddAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza preferências de notificação existentes.
    /// </summary>
    void Update(NotificationPreferences preferences);
}