namespace Bcommerce.BuildingBlocks.Security.Authentication;

/// <summary>
/// Configurações para geração e validação de JWT.
/// </summary>
/// <remarks>
/// Mapeia as configurações do `appsettings.json`.
/// - Define segredo, emissor e audiência
/// - Configura tempo de expiração do token
/// 
/// Exemplo de uso:
/// <code>
/// var secret = _jwtSettings.Secret;
/// </code>
/// </remarks>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}
