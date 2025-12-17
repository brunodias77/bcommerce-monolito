using System.Security.Claims;

namespace Bcommerce.BuildingBlocks.Security.Authentication;

/// <summary>
/// Contrato para geradores de tokens de autenticação.
/// </summary>
/// <remarks>
/// Responsável pela criação de tokens de acesso (ex: JWT).
/// - Encapsula a lógica de claims e assinatura
/// - Permite troca de estratégia de tokenização
/// 
/// Exemplo de uso:
/// <code>
/// var token = _tokenGenerator.GenerateToken(userId, ...);
/// </code>
/// </remarks>
public interface ITokenGenerator
{
    string GenerateToken(Guid userId, string firstName, string lastName, string email, IEnumerable<string> permissions, IEnumerable<string> roles);
}
