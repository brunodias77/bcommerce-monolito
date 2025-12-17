using Bcommerce.BuildingBlocks.Security.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Bcommerce.BuildingBlocks.Security.Authorization.Handlers;

/// <summary>
/// Handler de autorização para validar acesso a módulos.
/// </summary>
/// <remarks>
/// Verifica se o usuário possui a claim necessária para acessar o módulo.
/// - Implementa lógica de ModuleAccessRequirement
/// - Valida permissões a nível de aplicação/módulo
/// 
/// Exemplo de uso:
/// <code>
/// // Registrado automaticamente no DI
/// </code>
/// </remarks>
public class ModuleAccessHandler : AuthorizationHandler<ModuleAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleAccessRequirement requirement)
    {
        // Example implementation: Check if user has a role or claim for the module
        if (context.User.HasClaim(c => c.Type == "modules" && c.Value == requirement.ModuleName))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
