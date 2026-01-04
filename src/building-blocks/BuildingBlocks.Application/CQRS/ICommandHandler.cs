using BuildingBlocks.Application.Models;
using MediatR;

namespace BuildingBlocks.Application.CQRS;

/// <summary>
/// Interface para handlers de comandos (CQRS)
///
/// Handlers processam comandos e executam a lógica de negócio correspondente
/// Interagem com repositórios para persistir mudanças
/// Publicam eventos de domínio quando necessário
///
/// Responsabilidades:
/// - Validar o comando
/// - Executar a lógica de negócio
/// - Persistir mudanças no banco de dados
/// - Retornar Result indicando sucesso ou falha
///
/// Exemplos baseados no schema SQL:
///
/// CriarProdutoCommandHandler:
/// - Valida dados do produto (nome, preço, SKU único)
/// - Cria entidade Product
/// - Persiste usando catalog.products
/// - Publica ProdutoCriadoEvent
///
/// ProcessarPagamentoCommandHandler:
/// - Valida dados do pagamento
/// - Integra com gateway de pagamento
/// - Cria registro em payments.payments
/// - Atualiza status do pedido em orders.orders
/// - Publica PagamentoProcessadoEvent
/// </summary>
/// <typeparam name="TCommand">Tipo do comando</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Interface para handlers de comandos com valor de retorno tipado
/// </summary>
/// <typeparam name="TCommand">Tipo do comando</typeparam>
/// <typeparam name="TResponse">Tipo do valor de retorno</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}