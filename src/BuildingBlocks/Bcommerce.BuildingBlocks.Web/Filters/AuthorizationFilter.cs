using Microsoft.AspNetCore.Mvc.Filters;

namespace Bcommerce.BuildingBlocks.Web.Filters;

/// <summary>
/// Filtro customizado de autorização (exemplo/placeholder).
/// </summary>
/// <remarks>
/// Ponto de extensão para lógica de autorização customizada.
/// - Executa antes da Action
/// - Pode validar headers ou tokens específicos fora do padrão JwtBearer
/// 
/// Exemplo de uso:
/// <code>
/// [TypeFilter(typeof(AuthorizationFilter))]
/// </code>
/// </remarks>
public class AuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Implementação personalizada de autorização se necessário
        // Geralmente usamos o [Authorize] nativo do ASP.NET Core
    }
}
