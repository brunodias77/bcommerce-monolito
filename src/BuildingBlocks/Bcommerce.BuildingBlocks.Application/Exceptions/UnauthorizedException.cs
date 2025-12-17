namespace Bcommerce.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exceção para falha de autenticação (HTTP 401).
/// </summary>
/// <remarks>
/// Indica que o usuário não foi autenticado ou o token é inválido.
/// - Token ausente ou expirado
/// - Credenciais inválidas (Login)
/// - Deve forçar o usuário a fazer login novamente
/// 
/// Exemplo de uso:
/// <code>
/// if (tokenExpirado)
///     throw new UnauthorizedException("Token expirado.");
/// </code>
/// </remarks>
public class UnauthorizedException(string message) 
    : ApplicationException("Não Autorizado", message)
{
}
