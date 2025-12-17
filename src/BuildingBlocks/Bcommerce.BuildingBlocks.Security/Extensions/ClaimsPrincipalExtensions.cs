using System.Security.Claims;

namespace Bcommerce.BuildingBlocks.Security.Extensions;

/// <summary>
/// Métodos de extensão para ClaimsPrincipal.
/// </summary>
/// <remarks>
/// Facilita a extração de dados do usuário autenticado.
/// - Recupera o ID do usuário (sub) de forma tipada
/// - Reduz código repetitivo em controllers
/// 
/// Exemplo de uso:
/// <code>
/// var userId = HttpContext.User.GetUserId();
/// </code>
/// </remarks>
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst("sub")?.Value;
        return Guid.TryParse(userId, out var result) ? result : Guid.Empty;
    }
}
