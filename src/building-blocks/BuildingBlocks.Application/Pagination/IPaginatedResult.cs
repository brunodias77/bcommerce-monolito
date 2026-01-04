namespace BuildingBlocks.Application.Pagination;

/// <summary>
/// Interface para resultados paginados
///
/// Usado para retornar listas grandes de dados de forma eficiente
/// Baseado em queries que retornam muitos registros do schema SQL
///
/// Exemplos de uso:
///
/// Catálogo:
/// - Listar produtos por categoria (catalog.products)
/// - Buscar produtos por texto (usando índices pg_trgm)
/// - Listar avaliações de produto (catalog.product_reviews)
///
/// Pedidos:
/// - Listar pedidos do usuário (orders.orders)
/// - Histórico de status (orders.status_history)
/// - Eventos de rastreamento (orders.tracking_events)
///
/// Pagamentos:
/// - Listar pagamentos (payments.payments)
/// - Listar transações (payments.transactions)
/// - Histórico de chargebacks (payments.chargebacks)
///
/// Cupons:
/// - Listar cupons (coupons.coupons)
/// - Histórico de uso (coupons.usages)
///
/// Usuários:
/// - Histórico de login (users.login_history)
/// - Notificações (users.notifications)
/// - Lista de endereços (users.addresses)
/// </summary>
/// <typeparam name="T">Tipo dos itens na lista</typeparam>
public interface IPaginatedResult<out T>
{
    /// <summary>
    /// Itens da página atual
    /// </summary>
    IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Número da página atual (começa em 1)
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Tamanho da página (quantidade de itens por página)
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Total de itens em todas as páginas
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Indica se existe página anterior
    /// </summary>
    bool HasPrevious { get; }

    /// <summary>
    /// Indica se existe próxima página
    /// </summary>
    bool HasNext { get; }
}