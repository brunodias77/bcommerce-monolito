using BuildingBlocks.Domain.Repositories;
using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de sessões.
/// </summary>
public interface ISessionRepository : IRepository<Session>
{
    /// <summary>
    /// Busca uma sessão por ID.
    /// </summary>
    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca uma sessão por refresh token hash.
    /// </summary>
    Task<Session?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todas as sessões ativas de um usuário.
    /// </summary>
    Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca a sessão atual de um usuário.
    /// </summary>
    Task<Session?> GetCurrentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova sessão.
    /// </summary>
    Task AddAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma sessão existente.
    /// </summary>
    void Update(Session session);

    /// <summary>
    /// Remove uma sessão.
    /// </summary>
    void Remove(Session session);
}
