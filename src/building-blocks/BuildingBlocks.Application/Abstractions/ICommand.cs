using BuildingBlocks.Application.Results;
using MediatR;

namespace BuildingBlocks.Application.Abstractions;

/// <summary>
/// Interface para comandos (write operations) no padrão CQRS.
/// Comandos modificam o estado do sistema e retornam Result sem valor.
/// </summary>
/// <remarks>
/// Comandos são usados para:
/// - Criar, atualizar ou deletar entidades
/// - Executar operações de negócio que modificam estado
/// - Publicar eventos de domínio
/// 
/// Exemplo de uso:
/// <code>
/// public record CreateProductCommand(
///     string Sku,
///     string Name,
///     decimal Price,
///     int InitialStock
/// ) : ICommand;
/// 
/// internal class CreateProductCommandHandler : ICommandHandler&lt;CreateProductCommand&gt;
/// {
///     public async Task&lt;Result&gt; Handle(CreateProductCommand command, CancellationToken ct)
///     {
///         var product = Product.Create(command.Sku, command.Name, command.Price, command.InitialStock);
///         await _repository.AddAsync(product, ct);
///         await _unitOfWork.SaveChangesAsync(ct);
///         return Result.Ok();
///     }
/// }
/// </code>
/// </remarks>
public interface ICommand : IRequest<Result>
{
    // A interface ICommand herda de IRequest<Result> do MediatR.
    // Isso impõe que todo comando deve retornar um objeto Result.
    // O Result encapsula sucesso ou falha, evitando o uso excessivo de Exceptions para controle de fluxo.
}

/// <summary>
/// Interface para comandos que retornam um valor específico.
/// </summary>
/// <typeparam name="TResponse">Tipo do valor retornado no Result</typeparam>
/// <remarks>
/// Use quando o comando precisa retornar dados após a operação:
/// - Id da entidade criada
/// - Número do pedido gerado
/// - Token de confirmação
/// 
/// Exemplo de uso:
/// <code>
/// public record CreateOrderCommand(
///     Guid UserId,
///     List&lt;OrderItemDto&gt; Items
/// ) : ICommand&lt;Guid&gt;; // Retorna Id do pedido criado
/// 
/// internal class CreateOrderCommandHandler : ICommandHandler&lt;CreateOrderCommand, Guid&gt;
/// {
///     public async Task&lt;Result&lt;Guid&gt;&gt; Handle(CreateOrderCommand command, CancellationToken ct)
///     {
///         var order = Order.Create(command.UserId, command.Items);
///         await _repository.AddAsync(order, ct);
///         await _unitOfWork.SaveChangesAsync(ct);
///         return Result.Ok(order.Id);
///     }
/// }
/// </code>
/// </remarks>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}