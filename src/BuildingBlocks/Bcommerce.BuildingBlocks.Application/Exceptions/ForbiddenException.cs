namespace Bcommerce.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exceção para acesso proibido (HTTP 403).
/// </summary>
/// <remarks>
/// Indica que o usuário está autenticado mas não tem permissão suficiente.
/// - Falta de roles específica (ex: Admin)
/// - Tentativa de acessar recurso de outro tenant/usuário
/// - Diferente de 401 (Não Autenticado)
/// 
/// Exemplo de uso:
/// <code>
/// if (!usuario.IsAdmin)
///     throw new ForbiddenException("Acesso restrito a administradores.");
/// </code>
/// </remarks>
public class ForbiddenException(string message) 
    : ApplicationException("Proibido", message)
{
}
