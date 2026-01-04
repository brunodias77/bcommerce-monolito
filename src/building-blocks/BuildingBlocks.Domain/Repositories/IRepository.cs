using System.Linq.Expressions;
using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.Repositories;

/// <summary>
/// Interface genérica de repositório para operações CRUD
///
/// O padrão Repository abstrai a lógica de acesso a dados do domínio,
/// permitindo que o domínio trabalhe com coleções de objetos sem conhecer
/// detalhes de persistência
///
/// Baseado nas tabelas do schema SQL que contêm:
/// - Chaves primárias UUID
/// - Campos de auditoria (created_at, updated_at)
/// - Soft delete (deleted_at)
/// - Versionamento otimista (version)
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade</typeparam>
/// <typeparam name="TId">Tipo do identificador</typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Obtém uma entidade por ID
    /// </summary>
    /// <param name="id">Identificador da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de todas as entidades</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades que satisfazem uma condição
    /// </summary>
    /// <param name="predicate">Expressão de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades que satisfazem a condição</returns>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca uma única entidade que satisfaz uma condição
    /// </summary>
    /// <param name="predicate">Expressão de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe alguma entidade que satisfaz uma condição
    /// </summary>
    /// <param name="predicate">Expressão de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe, False caso contrário</returns>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta o número de entidades que satisfazem uma condição
    /// </summary>
    /// <param name="predicate">Expressão de filtro (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de entidades</returns>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova entidade
    /// </summary>
    /// <param name="entity">Entidade a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona múltiplas entidades
    /// </summary>
    /// <param name="entities">Entidades a serem adicionadas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    /// <param name="entity">Entidade a ser atualizada</param>
    void Update(TEntity entity);

    /// <summary>
    /// Atualiza múltiplas entidades
    /// </summary>
    /// <param name="entities">Entidades a serem atualizadas</param>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Remove uma entidade
    /// </summary>
    /// <param name="entity">Entidade a ser removida</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Remove uma entidade por ID
    /// </summary>
    /// <param name="id">Identificador da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoveByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove múltiplas entidades
    /// </summary>
    /// <param name="entities">Entidades a serem removidas</param>
    void RemoveRange(IEnumerable<TEntity> entities);
}
