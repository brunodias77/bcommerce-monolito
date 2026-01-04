using BuildingBlocks.Security.Authorization;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security;

/// <summary>
/// Extensões para configurar os serviços de segurança
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços de segurança ao container de DI
    /// Configura autenticação JWT, autorização e serviços de usuário atual
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Adiciona autenticação JWT
        services.AddJwtAuthentication(configuration);

        // Adiciona autorização com políticas baseadas em escopos
        services.AddCustomAuthorization(configuration);

        // Adiciona HttpContextAccessor para acessar o usuário atual
        services.AddHttpContextAccessor();

        // Registra serviço de usuário atual
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Adiciona políticas de autorização customizadas
    /// </summary>
    private static IServiceCollection AddCustomAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Carrega configurações de JWT para obter o Issuer
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthorization(options =>
        {
            // Políticas de catálogo
            options.AddPolicy("RequireCatalogReadScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("catalog:products:read", jwtOptions.Issuer)));

            options.AddPolicy("RequireCatalogWriteScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("catalog:products:write", jwtOptions.Issuer)));

            options.AddPolicy("RequireCatalogDeleteScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("catalog:products:delete", jwtOptions.Issuer)));

            options.AddPolicy("RequireStockManageScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("catalog:stock:manage", jwtOptions.Issuer)));

            // Políticas de pedidos
            options.AddPolicy("RequireOrdersReadScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("orders:read", jwtOptions.Issuer)));

            options.AddPolicy("RequireOrdersWriteScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("orders:write", jwtOptions.Issuer)));

            options.AddPolicy("RequireOrdersAdminScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("orders:admin", jwtOptions.Issuer)));

            // Políticas de pagamentos
            options.AddPolicy("RequirePaymentsProcessScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("payments:process", jwtOptions.Issuer)));

            options.AddPolicy("RequirePaymentsRefundScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("payments:refund", jwtOptions.Issuer)));

            // Políticas de cupons
            options.AddPolicy("RequireCouponsManageScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("coupons:manage", jwtOptions.Issuer)));

            // Políticas de usuários
            options.AddPolicy("RequireUsersManageScope",
                policy => policy.Requirements.Add(
                    new HasScopeRequirement("users:manage", jwtOptions.Issuer)));

            // Políticas baseadas em roles
            options.AddPolicy("RequireAdminRole",
                policy => policy.RequireRole("Admin"));

            options.AddPolicy("RequireManagerRole",
                policy => policy.RequireRole("Manager", "Admin"));

            options.AddPolicy("RequireCustomerRole",
                policy => policy.RequireRole("Customer", "Manager", "Admin"));
        });

        // Registra handler para validação de escopos
        services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

        return services;
    }
}

/// <summary>
/// Implementação de ICurrentUser que obtém informações do HttpContext
/// </summary>
internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => User?.GetUserId();

    public string? Email => User?.GetEmail();

    public string? UserName => User?.GetUserName();

    public string? FullName => User?.GetFullName();

    public bool IsAuthenticated => User?.IsAuthenticated() ?? false;

    public IEnumerable<string> Roles => User?.GetRoles() ?? Enumerable.Empty<string>();

    public IEnumerable<string> Scopes => User?.GetScopes() ?? Enumerable.Empty<string>();

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) ?? false;
    }

    public bool HasScope(string scope)
    {
        return User?.HasScope(scope) ?? false;
    }

    public string? GetClaimValue(string claimType)
    {
        return User?.GetClaimValue(claimType);
    }

    public IEnumerable<string> GetClaimValues(string claimType)
    {
        return User?.GetClaimValues(claimType) ?? Enumerable.Empty<string>();
    }
}
