using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.CQRS;

/// <summary>
/// Interface marcadora para queries (CQRS)
///
/// Queries representam CONSULTAS ao sistema que NÃO modificam estado
/// Sempre retornam Result com dados solicitados
///
/// Exemplos de queries baseados no schema SQL:
///
/// Catálogo:
/// - ObterProdutoPorIdQuery
/// - ListarProdutosPorCategoriaQuery
/// - BuscarProdutosPorTextoQuery (usando pg_trgm)
/// - ObterEstatisticasProdutoQuery (usando catalog.mv_product_stats)
/// - ListarProdutosFavoritosQuery
///
/// Pedidos:
/// - ObterPedidoPorIdQuery
/// - ListarPedidosDoUsuarioQuery
/// - ObterDetalhesPedidoQuery
/// - ListarPedidosPorStatusQuery
/// - ObterHistoricoRastreamentoQuery
///
/// Pagamentos:
/// - ObterPagamentoPorIdQuery
/// - ListarPagamentosDoUsuarioQuery
/// - ObterMetodosPagamentoSalvosQuery
/// - ObterStatusPagamentoQuery
///
/// Cupons:
/// - ValidarCupomQuery
/// - ListarCuponsAtivosQuery (usando coupons.v_active_coupons)
/// - ObterMetricasCupomQuery (usando coupons.v_coupon_metrics)
/// - VerificarElegibilidadeCupomQuery
///
/// Carrinho:
/// - ObterCarrinhoAtivoQuery
/// - ObterTotaisCarrinhoQuery (usando cart.calculate_cart_totals)
/// - ListarCarrinhosSalvosQuery
/// - DetectarCarrinhosAbandonadosQuery (usando cart.v_abandoned_carts)
///
/// Usuários:
/// - ObterPerfilUsuarioQuery
/// - ListarEnderecosQuery
/// - ObterNotificacoesNaoLidasQuery (usando users.v_unread_notifications)
/// - ListarSessoesAtivasQuery (usando users.v_active_sessions)
/// </summary>
/// <typeparam name="TResponse">Tipo do dado retornado pela query</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}