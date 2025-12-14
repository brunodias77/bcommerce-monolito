using BuildingBlocks.Domain.Repositories;
using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de perfis.
/// </summary>
public interface IProfileRepository : IRepository<Profile>
{
    /// <summary>
    /// Busca um perfil por ID.
    /// </summary>
    Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um perfil por user ID.
    /// </summary>
    Task<Profile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um perfil por CPF.
    /// </summary>
    Task<Profile?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um CPF já está cadastrado.
    /// </summary>
    Task<bool> CpfExistsAsync(string cpf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo perfil.
    /// </summary>
    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um perfil existente.
    /// </summary>
    void Update(Profile profile);

    /// <summary>
    /// Remove um perfil (soft delete).
    /// </summary>
    void Remove(Profile profile);
}