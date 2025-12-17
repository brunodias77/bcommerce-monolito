using System.Security.Claims;

namespace Bcommerce.BuildingBlocks.Security.Services;

/// <summary>
/// Contrato para acesso às informações do usuário logado (Contexto).
/// </summary>
/// <remarks>
/// Abstrai o acesso ao HttpContext.
/// - Fornece o ID e Principal do usuário atual
/// - Facilita testes unitários ao evitar dependência direta de HttpContext
/// 
/// Exemplo de uso:
/// <code>
/// if (_currentUserService.UserId == order.OwnerId) ...
/// </code>
/// </remarks>
public interface ICurrentUserService
{
    Guid UserId { get; }
    ClaimsPrincipal? User { get; }
}
