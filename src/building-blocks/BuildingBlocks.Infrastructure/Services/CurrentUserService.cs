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
/// Implementação fake do ICurrentUserService para testes.
/// </summary>
/// <remarks>
/// Use esta implementação em testes unitários para simular diferentes cenários:
///
/// <code>
/// // Teste com usuário autenticado
/// var currentUser = new FakeCurrentUserService(
///     userId: Guid.NewGuid(),
///     email: "test@example.com"
/// );
///
/// // Teste com usuário não autenticado
/// var anonymousUser = new FakeCurrentUserService();
///
/// // Teste alterando o usuário durante o teste
/// var currentUser = new FakeCurrentUserService();
/// currentUser.SetUser(Guid.NewGuid(), "user1@example.com");
/// // ... executar código
/// currentUser.SetUser(Guid.NewGuid(), "user2@example.com");
/// // ... executar mais código
/// currentUser.SetAnonymous();
/// // ... testar cenário não autenticado
/// </code>
/// </remarks>
public class FakeCurrentUserService : ICurrentUserService
{
    private Guid? _userId;
    private string? _email;
    private bool _isAuthenticated;

    /// <summary>
    /// Cria um usuário fake não autenticado.
    /// </summary>
    public FakeCurrentUserService()
    {
        _isAuthenticated = false;
    }

    /// <summary>
    /// Cria um usuário fake autenticado.
    /// </summary>
    public FakeCurrentUserService(Guid userId, string email)
    {
        _userId = userId;
        _email = email;
        _isAuthenticated = true;
    }

    public Guid? UserId => _userId;
    public string? Email => _email;
    public bool IsAuthenticated => _isAuthenticated;

    /// <summary>
    /// Define um usuário autenticado.
    /// </summary>
    public void SetUser(Guid userId, string email)
    {
        _userId = userId;
        _email = email;
        _isAuthenticated = true;
    }

    /// <summary>
    /// Define o estado como não autenticado.
    /// </summary>
    public void SetAnonymous()
    {
        _userId = null;
        _email = null;
        _isAuthenticated = false;
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

    /// <summary>
    /// Registra um FakeCurrentUserService para testes.
    /// </summary>
    public static IServiceCollection AddFakeCurrentUserService(
        this IServiceCollection services,
        Guid? userId = null,
        string? email = null)
    {
        var fakeService = userId.HasValue && email != null
            ? new FakeCurrentUserService(userId.Value, email)
            : new FakeCurrentUserService();

        services.AddSingleton<ICurrentUserService>(fakeService);
        return services;
    }
}
