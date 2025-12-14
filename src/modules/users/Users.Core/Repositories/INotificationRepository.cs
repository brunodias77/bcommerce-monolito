using BuildingBlocks.Domain.Repositories;
using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de notificações.
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>
    /// Busca uma notificação por ID.
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca notificações de um usuário com paginação.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca notificações não lidas de um usuário.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta notificações não lidas de um usuário.
    /// </summary>
    Task<int> CountUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova notificação.
    /// </summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma notificação existente.
    /// </summary>
    void Update(Notification notification);

    /// <summary>
    /// Remove uma notificação.
    /// </summary>
    void Remove(Notification notification);
}
