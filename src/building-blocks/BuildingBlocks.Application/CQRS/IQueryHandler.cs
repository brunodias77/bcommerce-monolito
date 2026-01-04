using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.CQRS;

/// <summary>
/// Interface para handlers de queries (CQRS)
///
/// Handlers processam queries e retornam dados solicitados
/// NÃO devem modificar o estado do sistema
/// Podem usar queries otimizadas, views materializadas e índices
///
/// Responsabilidades:
/// - Validar os parâmetros da query
/// - Buscar dados do banco (usando repositórios, views, ou queries diretas)
/// - Mapear entidades para DTOs
/// - Retornar Result com os dados ou erro
///
/// Otimizações baseadas no schema SQL:
///
/// Para catálogo:
/// - Usar catalog.mv_product_stats para estatísticas de produtos (pré-calculado)
/// - Usar índices GIN para busca por tags e atributos JSONB
/// - Usar índices pg_trgm para busca textual por nome
///
/// Para carrinhos:
/// - Usar cart.v_active_carts para carrinho com totais
/// - Usar cart.v_abandoned_carts para detecção de abandono
/// - Usar função cart.calculate_cart_totals para cálculos
///
/// Para pedidos:
/// - Usar orders.v_user_order_summary para resumo do usuário
/// - Usar orders.v_orders_pending_action para alertas
/// - Usar índices em order_number e tracking_code
///
/// Para cupons:
/// - Usar coupons.v_active_coupons para cupons válidos
/// - Usar coupons.v_coupon_metrics para métricas
/// - Usar função coupons.validate_coupon_usage para validação
/// </summary>
/// <typeparam name="TQuery">Tipo da query</typeparam>
/// <typeparam name="TResponse">Tipo do dado retornado</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}