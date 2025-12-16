using Microsoft.AspNetCore.Mvc.Filters;

namespace Bcommerce.BuildingBlocks.Web.Filters;

public class AuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Implementação personalizada de autorização se necessário
        // Geralmente usamos o [Authorize] nativo do ASP.NET Core
    }
}
