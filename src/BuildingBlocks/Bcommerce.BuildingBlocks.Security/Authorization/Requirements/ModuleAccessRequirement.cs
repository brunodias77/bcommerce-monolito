using Microsoft.AspNetCore.Authorization;

namespace Bcommerce.BuildingBlocks.Security.Authorization.Requirements;

/// <summary>
/// Requisito de autorização para acesso a módulos.
/// </summary>
/// <remarks>
/// Define a necessidade de acesso a um módulo específico.
/// - Usado em conjunto com ModuleAccessHandler
/// - Transporta o nome do módulo validador
/// 
/// Exemplo de uso:
/// <code>
/// new ModuleAccessRequirement("Catalog");
/// </code>
/// </remarks>
public class ModuleAccessRequirement : IAuthorizationRequirement
{
    public string ModuleName { get; }

    public ModuleAccessRequirement(string moduleName)
    {
        ModuleName = moduleName;
    }
}
