using Users.Core.Entities;

namespace Users.Core.Repositories;

/// <summary>
/// Interface do repositório de usuários.
/// Não herda de IRepository<User> porque User herda de IdentityUser<Guid> (ASP.NET Identity)
/// em vez de Entity (BuildingBlocks.Domain).
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Busca um usuário por ID.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário por email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário por username.
    /// </summary>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário com perfil incluído.
    /// </summary>
    Task<User?> GetByIdWithProfileAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário com todos os relacionamentos.
    /// </summary>
    Task<User?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um email já está em uso.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um username já está em uso.
    /// </summary>
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo usuário.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um usuário existente.
    /// </summary>
    void Update(User user);

    /// <summary>
    /// Remove um usuário.
    /// </summary>
    void Remove(User user);
}