using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BuildingBlocks.Security.Authorization;

/// <summary>
/// Extensões para facilitar leitura de Claims do ClaimsPrincipal
///
/// ClaimsPrincipal representa o usuário autenticado e contém todos os claims (informações)
/// extraídos do JWT token
///
/// Claims comuns baseados no JWT:
/// - sub (subject): ID do usuário (users.asp_net_users.id)
/// - email: Email do usuário
/// - name: Nome de usuário
/// - role: Papel(is) do usuário (users.asp_net_roles)
/// - scope: Escopos/permissões
/// - jti: JWT ID (identificador único do token)
/// - iat: Issued At (quando o token foi criado)
/// - exp: Expiration (quando o token expira)
/// - iss: Issuer (emissor do token)
/// - aud: Audience (audiência do token)
///
/// Uso em controllers/handlers:
///
/// public class MeuController : ControllerBase
/// {
///     [Authorize]
///     [HttpGet]
///     public IActionResult MeuEndpoint()
///     {
///         // Obtém ID do usuário logado
///         var userId = User.GetUserId();
///
///         // Obtém email
///         var email = User.GetEmail();
///
///         // Verifica se tem permissão
///         if (!User.HasScope("catalog:products:write"))
///         {
///             return Forbid();
///         }
///
///         // Verifica múltiplas roles
///         if (!User.IsInAnyRole("Admin", "Manager"))
///         {
///             return Forbid();
///         }
///
///         return Ok();
///     }
/// }
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Obtém o ID do usuário (Guid)
    /// Claim: "sub" (subject)
    /// Corresponde a users.asp_net_users.id
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)
                       ?? principal.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return null;
        }

        return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    /// <summary>
    /// Obtém o email do usuário
    /// Claim: "email"
    /// Corresponde a users.asp_net_users.email
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Obtém o nome de usuário
    /// Claim: "name"
    /// Corresponde a users.asp_net_users.user_name
    /// </summary>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value
            ?? principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("preferred_username")?.Value;
    }

    /// <summary>
    /// Obtém o nome completo do usuário
    /// Pode vir de users.profiles (first_name + last_name)
    /// </summary>
    public static string? GetFullName(this ClaimsPrincipal principal)
    {
        var givenName = principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value
                     ?? principal.FindFirst(ClaimTypes.GivenName)?.Value;

        var familyName = principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value
                      ?? principal.FindFirst(ClaimTypes.Surname)?.Value;

        if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(familyName))
        {
            return $"{givenName} {familyName}";
        }

        return givenName ?? familyName;
    }

    /// <summary>
    /// Obtém todos os papéis (roles) do usuário
    /// Claims: "role"
    /// Corresponde a users.asp_net_user_roles
    /// </summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct();
    }

    /// <summary>
    /// Verifica se o usuário possui um papel específico
    /// </summary>
    /// <param name="principal">Principal do usuário</param>
    /// <param name="role">Nome do papel (Admin, Customer, Manager)</param>
    public static bool IsInRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }

    /// <summary>
    /// Verifica se o usuário possui pelo menos um dos papéis especificados
    /// </summary>
    /// <param name="principal">Principal do usuário</param>
    /// <param name="roles">Papéis a verificar</param>
    public static bool IsInAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.Any(role => principal.IsInRole(role));
    }

    /// <summary>
    /// Verifica se o usuário possui todos os papéis especificados
    /// </summary>
    /// <param name="principal">Principal do usuário</param>
    /// <param name="roles">Papéis a verificar</param>
    public static bool IsInAllRoles(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.All(role => principal.IsInRole(role));
    }

    /// <summary>
    /// Obtém todos os escopos do usuário
    /// Claims: "scope"
    /// Permissões granulares (catalog:products:write, orders:admin, etc.)
    /// </summary>
    public static IEnumerable<string> GetScopes(this ClaimsPrincipal principal)
    {
        return principal.FindAll("scope")
            .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct();
    }

    /// <summary>
    /// Verifica se o usuário possui um escopo específico
    /// </summary>
    /// <param name="principal">Principal do usuário</param>
    /// <param name="scope">Escopo a verificar (ex: "catalog:products:write")</param>
    public static bool HasScope(this ClaimsPrincipal principal, string scope)
    {
        var scopes = GetScopes(principal);
        return scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica se o usuário possui pelo menos um dos escopos especificados
    /// </summary>
    public static bool HasAnyScope(this ClaimsPrincipal principal, params string[] scopes)
    {
        var userScopes = GetScopes(principal).ToList();
        return scopes.Any(scope => userScopes.Contains(scope, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica se o usuário possui todos os escopos especificados
    /// </summary>
    public static bool HasAllScopes(this ClaimsPrincipal principal, params string[] scopes)
    {
        var userScopes = GetScopes(principal).ToList();
        return scopes.All(scope => userScopes.Contains(scope, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtém o valor de um claim específico
    /// </summary>
    /// <param name="principal">Principal do usuário</param>
    /// <param name="claimType">Tipo do claim</param>
    public static string? GetClaimValue(this ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Obtém todos os valores de um claim (para claims com múltiplos valores)
    /// </summary>
    /// <param name="principal">Principal do usuário</param>
    /// <param name="claimType">Tipo do claim</param>
    public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal principal, string claimType)
    {
        return principal.FindAll(claimType)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v));
    }

    /// <summary>
    /// Obtém o ID do JWT (jti - JWT ID)
    /// Útil para rastreamento e invalidação de tokens
    /// </summary>
    public static string? GetJwtId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
    }

    /// <summary>
    /// Obtém a data de expiração do token
    /// Claim: "exp"
    /// </summary>
    public static DateTime? GetTokenExpiration(this ClaimsPrincipal principal)
    {
        var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (expClaim == null || !long.TryParse(expClaim, out var exp))
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
    }

    /// <summary>
    /// Obtém a data de emissão do token
    /// Claim: "iat" (issued at)
    /// </summary>
    public static DateTime? GetTokenIssuedAt(this ClaimsPrincipal principal)
    {
        var iatClaim = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;

        if (iatClaim == null || !long.TryParse(iatClaim, out var iat))
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;
    }

    /// <summary>
    /// Verifica se o token ainda é válido (não expirou)
    /// </summary>
    public static bool IsTokenValid(this ClaimsPrincipal principal)
    {
        var expiration = GetTokenExpiration(principal);
        return expiration.HasValue && expiration.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se o usuário está autenticado
    /// </summary>
    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated ?? false;
    }
}
