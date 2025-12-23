using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Bcommerce.BuildingBlocks.Security.Services;

/// <summary>
/// Implementação do serviço de acesso ao usuário atual.
/// </summary>
/// <remarks>
/// Utiliza IHttpContextAccessor para obter dados do usuário da requisição.
/// - Implementa ICurrentUserService
/// - Resolve UserId a partir das claims (sub) de forma segura
/// 
/// Exemplo de uso:
/// <code>
/// var currentUser = _currentUserService.UserId;
/// </code>
/// </remarks>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(userId, out var result) ? result : Guid.Empty;
        }
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
}
