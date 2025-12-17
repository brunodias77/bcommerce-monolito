using Microsoft.AspNetCore.Authorization;

namespace Bcommerce.BuildingBlocks.Security.Authorization.Requirements;

/// <summary>
/// Requisito de autorização para permissões específicas.
/// </summary>
/// <remarks>
/// Define a exigência de uma permissão pontual.
/// - Base para construção de policies dinâmicas
/// - Verificado por PermissionHandler
/// 
/// Exemplo de uso:
/// <code>
/// new PermissionRequirement(Permissions.Write);
/// </code>
/// </remarks>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
