using BuildingBlocks.Domain.Repositories;
using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de histórico de login.
/// </summary>
public interface ILoginHistoryRepository : IRepository<LoginHistory>
{
    /// <summary>
    /// Busca histórico de login de um usuário com paginação.
    /// </summary>
    Task<IReadOnlyList<LoginHistory>> GetByUserIdAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca tentativas de login falhadas recentes de um usuário.
    /// </summary>
    Task<IReadOnlyList<LoginHistory>> GetRecentFailedByUserIdAsync(
        Guid userId,
        int minutes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo registro de login.
    /// </summary>
    Task AddAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default);
}
