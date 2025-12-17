namespace Bcommerce.BuildingBlocks.Application.Abstractions.Services;

/// <summary>
/// Abstração para acessar o contexto do usuário atual (geralmente via ClaimsPrincipal).
/// </summary>
/// <remarks>
/// Desacopla a camada de aplicação de frameworks web (HttpContext).
/// - Fornece acesso ao ID e Email do usuário logado
/// - Permite verificação de permissões e roles
/// - Facilita testes unitários ao evitar dependência de HttpContext
/// 
/// Exemplo de uso:
/// <code>
/// public class CriarPedidoHandler : ICommandHandler&lt;CriarPedidoCommand&gt;
/// {
///     private readonly ICurrentUserService _user;
///     
///     public async Task Handle(CriarPedidoCommand cmd, CancellationToken ct)
///     {
///         var userId = _user.UserId ?? throw new UnauthorizedAccessException();
///         // ...
///     }
/// }
/// </code>
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>Identificador único do usuário logado (Subject ID).</summary>
    Guid? UserId { get; }
    /// <summary>Email principal do usuário logado.</summary>
    string? Email { get; }
    /// <summary>Indica se existe um usuário autenticado no contexto atual.</summary>
    bool IsAuthenticated { get; }
    /// <summary>
    /// Verifica se o usuário possui uma role específica de acesso.
    /// </summary>
    /// <param name="role">Nome da role a ser verificada.</param>
    /// <returns>True se o usuário possuir a role, False caso contrário.</returns>
    bool IsInRole(string role);
}
