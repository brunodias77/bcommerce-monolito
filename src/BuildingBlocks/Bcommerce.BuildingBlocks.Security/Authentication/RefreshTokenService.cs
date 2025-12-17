using System.Security.Cryptography;

namespace Bcommerce.BuildingBlocks.Security.Authentication;

/// <summary>
/// Serviço para gerenciamento de Refresh Tokens.
/// </summary>
/// <remarks>
/// Gera tokens opacos seguros para renovação de acesso.
/// - Utiliza gerador de números aleatórios criptograficamente seguro
/// - Produz strings Base64
/// 
/// Exemplo de uso:
/// <code>
/// var refreshToken = _refreshTokenService.GenerateRefreshToken();
/// </code>
/// </remarks>
public class RefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
