namespace Bcommerce.BuildingBlocks.Security.Authorization.Policies;

/// <summary>
/// Constantes para nomes de políticas de autorização.
/// </summary>
/// <remarks>
/// Garante consistência nos nomes das Policies.
/// - Evita erros de digitação em atributos Authorize
/// - Mapeia regras de negócio para identificadores técnicos
/// 
/// Exemplo de uso:
/// <code>
/// options.AddPolicy(PolicyNames.HasPermission, ...);
/// </code>
/// </remarks>
public static class PolicyNames
{
    public const string HasPermission = "HasPermission";
    public const string ModuleAccess = "ModuleAccess";
}
