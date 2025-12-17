namespace Bcommerce.BuildingBlocks.Security.Authorization.Policies;

/// <summary>
/// Constantes de permissões do sistema.
/// </summary>
/// <remarks>
/// Centraliza as strings de permissão para evitar hardcoding.
/// - Define ações como Read, Write, Delete
/// - Usado para configurar Policies e Claims
/// 
/// Exemplo de uso:
/// <code>
/// [Authorize(Policy = Permissions.Read)]
/// </code>
/// </remarks>
public static class Permissions
{
    // Example permissions, extend as needed
    public const string Read = "read";
    public const string Write = "write";
    public const string Delete = "delete";
}
