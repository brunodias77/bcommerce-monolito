namespace BuildingBlocks.Security.Services;

/// <summary>
/// Interface para obter informações do usuário autenticado atual
/// Abstrai o acesso aos Claims do JWT token
///
/// Baseado no schema SQL - tabela users.asp_net_users:
/// - Id (Guid) - Identificador único do usuário
/// - UserName (string) - Nome de usuário
/// - Email (string) - Email do usuário
/// - Roles (lista) - Papéis/permissões do usuário (Admin, Customer, Manager)
///
/// Claims comuns no JWT token:
/// - sub (subject): ID do usuário
/// - email: Email do usuário
/// - name: Nome do usuário
/// - role: Papel do usuário (pode ter múltiplos)
/// - scope: Escopos/permissões específicas
///
/// Uso em Handlers/Services:
///
/// Exemplo 1 - Listar pedidos do usuário logado:
/// public class ListarPedidosDoUsuarioQueryHandler
/// {
///     private readonly ICurrentUser _currentUser;
///
///     public async Task<Result> Handle(...)
///     {
///         var usuarioId = _currentUser.UserId; // Obtém ID do usuário logado
///         var pedidos = await _repository.GetByUserIdAsync(usuarioId);
///         return Result.Success(pedidos);
///     }
/// }
///
/// Exemplo 2 - Criar pedido para o usuário logado:
/// public class CriarPedidoCommandHandler
/// {
///     public async Task<Result> Handle(CriarPedidoCommand command, ...)
///     {
///         // Garante que o pedido pertence ao usuário logado
///         var pedido = new Order(
///             userId: _currentUser.UserId,
///             items: command.Items,
///             ...
///         );
///
///         await _repository.AddAsync(pedido);
///         return Result.Success();
///     }
/// }
///
/// Exemplo 3 - Verificar permissões:
/// public class ExcluirProdutoCommandHandler
/// {
///     public async Task<Result> Handle(...)
///     {
///         // Apenas admins podem excluir produtos
///         if (!_currentUser.IsInRole("Admin"))
///         {
///             return Result.Failure(Error.Forbidden(
///                 "SEM_PERMISSAO",
///                 "Você não tem permissão para excluir produtos"));
///         }
///
///         await _repository.RemoveAsync(produtoId);
///         return Result.Success();
///     }
/// }
///
/// Exemplo 4 - Auditoria (AuditInterceptor):
/// public class AuditInterceptor
/// {
///     public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
///     {
///         foreach (var entry in context.ChangeTracker.Entries())
///         {
///             if (entry.State == EntityState.Added)
///             {
///                 entry.Property("CreatedBy").CurrentValue = _currentUser.UserId;
///             }
///
///             if (entry.State == EntityState.Modified)
///             {
///                 entry.Property("UpdatedBy").CurrentValue = _currentUser.UserId;
///             }
///         }
///     }
/// }
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// ID do usuário autenticado
    /// Corresponde ao campo 'id' em users.asp_net_users
    /// Claim: "sub" (subject)
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email do usuário autenticado
    /// Corresponde ao campo 'email' em users.asp_net_users
    /// Claim: "email"
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Nome de usuário
    /// Corresponde ao campo 'user_name' em users.asp_net_users
    /// Claim: "name" ou "preferred_username"
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Nome completo do usuário (se disponível)
    /// Pode vir de users.profiles (first_name + last_name)
    /// Claim: "given_name" e "family_name"
    /// </summary>
    string? FullName { get; }

    /// <summary>
    /// Indica se há um usuário autenticado
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Verifica se o usuário possui um papel específico
    /// Papéis baseados em users.asp_net_roles:
    /// - Admin: Acesso total ao sistema
    /// - Manager: Gerenciamento de produtos, pedidos
    /// - Customer: Compras e consultas
    /// </summary>
    /// <param name="role">Nome do papel (role)</param>
    /// <returns>True se o usuário possui o papel</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Verifica se o usuário possui um escopo/permissão específica
    /// Usado para permissões granulares (ex: "products:write", "orders:read")
    /// </summary>
    /// <param name="scope">Nome do escopo</param>
    /// <returns>True se o usuário possui o escopo</returns>
    bool HasScope(string scope);

    /// <summary>
    /// Obtém todos os papéis do usuário
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Obtém todos os escopos/permissões do usuário
    /// </summary>
    IEnumerable<string> Scopes { get; }

    /// <summary>
    /// Obtém o valor de um claim específico
    /// </summary>
    /// <param name="claimType">Tipo do claim</param>
    /// <returns>Valor do claim ou null</returns>
    string? GetClaimValue(string claimType);

    /// <summary>
    /// Obtém todos os valores de um claim (para claims com múltiplos valores)
    /// Exemplo: múltiplos roles
    /// </summary>
    /// <param name="claimType">Tipo do claim</param>
    /// <returns>Lista de valores do claim</returns>
    IEnumerable<string> GetClaimValues(string claimType);
}
