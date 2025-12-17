using Bcommerce.BuildingBlocks.Security.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Bcommerce.BuildingBlocks.Security.Authorization.Handlers;

/// <summary>
/// Handler de autorização para validar permissões granulares.
/// </summary>
/// <remarks>
/// Verifica se o usuário possui a permissão (claim) específica.
/// - Suporta validação de Policies baseadas em permissões
/// - Avalia claims do tipo "permissions"
/// 
/// Exemplo de uso:
/// <code>
/// // Utilizado internamente pelo Authorize(Policy = "...")
/// </code>
/// </remarks>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "permissions" && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
