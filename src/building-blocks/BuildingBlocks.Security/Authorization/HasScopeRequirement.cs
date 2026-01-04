using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security.Authorization;

/// <summary>
/// Requisito de autorização baseado em escopos (scopes)
///
/// Escopos são permissões granulares que permitem controlar acesso a recursos específicos
/// Mais específico que Roles (Admin, Customer, Manager)
///
/// Exemplos de escopos baseados no schema SQL:
///
/// Catálogo (catalog):
/// - "catalog:products:read" - Ler produtos
/// - "catalog:products:write" - Criar/editar produtos
/// - "catalog:products:delete" - Excluir produtos
/// - "catalog:categories:manage" - Gerenciar categorias
/// - "catalog:stock:manage" - Gerenciar estoque
///
/// Pedidos (orders):
/// - "orders:read" - Ler pedidos
/// - "orders:write" - Criar pedidos
/// - "orders:cancel" - Cancelar pedidos
/// - "orders:admin" - Administrar todos os pedidos
///
/// Pagamentos (payments):
/// - "payments:process" - Processar pagamentos
/// - "payments:refund" - Reembolsar pagamentos
/// - "payments:view_all" - Ver todos os pagamentos
///
/// Cupons (coupons):
/// - "coupons:create" - Criar cupons
/// - "coupons:edit" - Editar cupons
/// - "coupons:deactivate" - Desativar cupons
///
/// Usuários (users):
/// - "users:read" - Ler usuários
/// - "users:manage" - Gerenciar usuários
/// - "users:impersonate" - Personificar usuários (para suporte)
///
/// Uso em controllers:
///
/// [Authorize(Policy = "RequireCatalogWriteScope")]
/// [HttpPost("api/products")]
/// public async Task<IActionResult> CriarProduto(...)
/// {
///     // Apenas usuários com escopo "catalog:products:write"
///     // podem criar produtos
/// }
///
/// [Authorize(Policy = "RequireOrdersAdminScope")]
/// [HttpGet("api/admin/orders")]
/// public async Task<IActionResult> ListarTodosPedidos(...)
/// {
///     // Apenas usuários com escopo "orders:admin"
///     // podem ver todos os pedidos
/// }
///
/// Configuração de políticas no Program.cs:
///
/// builder.Services.AddAuthorization(options =>
/// {
///     // Catálogo
///     options.AddPolicy("RequireCatalogReadScope",
///         policy => policy.Requirements.Add(new HasScopeRequirement("catalog:products:read")));
///
///     options.AddPolicy("RequireCatalogWriteScope",
///         policy => policy.Requirements.Add(new HasScopeRequirement("catalog:products:write")));
///
///     // Pedidos
///     options.AddPolicy("RequireOrdersAdminScope",
///         policy => policy.Requirements.Add(new HasScopeRequirement("orders:admin")));
///
///     // Pagamentos
///     options.AddPolicy("RequirePaymentsProcessScope",
///         policy => policy.Requirements.Add(new HasScopeRequirement("payments:process")));
/// });
///
/// Atribuição de escopos:
///
/// Escopos são adicionados como claims no JWT token durante o login
/// Baseado em users.asp_net_user_claims ou users.asp_net_role_claims
///
/// Exemplo de claims no JWT:
/// {
///   "sub": "123e4567-e89b-12d3-a456-426614174000",
///   "email": "admin@bcommerce.com",
///   "role": "Admin",
///   "scope": [
///     "catalog:products:write",
///     "orders:admin",
///     "payments:process"
///   ]
/// }
/// </summary>
public sealed class HasScopeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Emissor do token (deve corresponder ao Jwt:Issuer)
    /// </summary>
    public string Issuer { get; }

    /// <summary>
    /// Escopo necessário para autorização
    /// </summary>
    public string Scope { get; }

    /// <summary>
    /// Cria um novo requisito de escopo
    /// </summary>
    /// <param name="scope">Escopo necessário (ex: "catalog:products:write")</param>
    /// <param name="issuer">Emissor do token (ex: "BCommerce")</param>
    public HasScopeRequirement(string scope, string issuer)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}

/// <summary>
/// Handler que valida se o usuário possui o escopo necessário
/// </summary>
public sealed class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasScopeRequirement requirement)
    {
        // Verifica se o usuário está autenticado
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // Valida o emissor (issuer)
        var issuerClaim = context.User.FindFirst(c => c.Type == "iss");

        if (issuerClaim == null || issuerClaim.Value != requirement.Issuer)
        {
            return Task.CompletedTask;
        }

        // Busca claims de escopo
        // Pode ser um claim com múltiplos valores ou múltiplos claims
        var scopeClaims = context.User
            .FindAll(c => c.Type == "scope")
            .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToList();

        // Verifica se o usuário possui o escopo necessário
        if (scopeClaims.Contains(requirement.Scope, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
