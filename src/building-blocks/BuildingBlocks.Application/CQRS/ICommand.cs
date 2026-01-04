using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.CQRS;

/// <summary>
/// Interface marcadora para comandos (CQRS)
///
/// Comandos representam INTENÇÕES de modificar o estado do sistema
/// Sempre retornam Result para indicar sucesso ou falha
///
/// Exemplos de comandos baseados no schema SQL:
///
/// Catálogo:
/// - CriarProdutoCommand
/// - AtualizarProdutoCommand
/// - ExcluirProdutoCommand
/// - ReservarEstoqueCommand
/// - LiberarEstoqueCommand
///
/// Pedidos:
/// - CriarPedidoCommand
/// - AtualizarStatusPedidoCommand
/// - CancelarPedidoCommand
/// - AdicionarRastreamentoCommand
///
/// Pagamentos:
/// - ProcessarPagamentoCommand
/// - CancelarPagamentoCommand
/// - ReembolsarPagamentoCommand
/// - SalvarMetodoPagamentoCommand
///
/// Cupons:
/// - CriarCupomCommand
/// - AtivarCupomCommand
/// - DesativarCupomCommand
/// - AplicarCupomCommand
///
/// Carrinho:
/// - AdicionarItemCarrinhoCommand
/// - RemoverItemCarrinhoCommand
/// - AtualizarQuantidadeCommand
/// - LimparCarrinhoCommand
/// - ConverterCarrinhoEmPedidoCommand
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Interface para comandos que retornam um valor tipado
/// </summary>
/// <typeparam name="TResponse">Tipo do valor de retorno em caso de sucesso</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}