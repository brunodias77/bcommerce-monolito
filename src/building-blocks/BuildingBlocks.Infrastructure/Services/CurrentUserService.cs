using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BuildingBlocks.Infrastructure.Services;

/// <summary>
/// Implementação padrão do ICurrentUserService usando HttpContext.
/// </summary>
/// <remarks>
/// Esta implementação:
/// - Acessa o HttpContext.User via IHttpContextAccessor
/// - Extrai claims do JWT token ou Cookie de autenticação
/// - Retorna null para propriedades se usuário não autenticado
///
/// Claims esperados:
/// - "sub" ou "userId": ID do usuário (Guid)
/// - "email": Email do usuário
///
/// Configuração no Startup:
/// <code>
/// services.AddHttpContextAccessor();
/// services.AddScoped&lt;ICurrentUserService, CurrentUserService&gt;();
/// </code>
/// </remarks>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Tenta obter o claim "sub" (padrão JWT)
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst("sub")
                ?? user.FindFirst("userId");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
    }

    public string? Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Tenta obter o claim "email"
            var emailClaim = user.FindFirst(ClaimTypes.Email)
                ?? user.FindFirst("email");

            return emailClaim?.Value;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true;
        }
    }
}

/// <summary>
/// Extensões para facilitar registro do CurrentUserService.
/// </summary>
public static class CurrentUserServiceExtensions
{
    /// <summary>
    /// Registra o CurrentUserService no DI.
    /// </summary>
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        // AddHttpContextAccessor deve ser chamado no projeto da API
        // services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
