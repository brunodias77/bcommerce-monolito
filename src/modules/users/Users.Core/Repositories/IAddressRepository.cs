using BuildingBlocks.Domain.Repositories;
using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de endereços.
/// </summary>
public interface IAddressRepository : IRepository<Address>
{
    /// <summary>
    /// Busca um endereço por ID.
    /// </summary>
    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todos os endereços de um usuário.
    /// </summary>
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca o endereço padrão de um usuário.
    /// </summary>
    Task<Address?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo endereço.
    /// </summary>
    Task AddAsync(Address address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um endereço existente.
    /// </summary>
    void Update(Address address);

    /// <summary>
    /// Remove um endereço (soft delete).
    /// </summary>
    void Remove(Address address);
}