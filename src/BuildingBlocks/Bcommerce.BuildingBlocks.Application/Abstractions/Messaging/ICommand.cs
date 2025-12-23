using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Contrato para comandos que não retornam dados (void), apenas sucesso/falha.
/// </summary>
/// <remarks>
/// Representa uma intenção de alterar o estado do sistema (Write Side).
/// - Implementa IRequest&lt;Result&gt; do MediatR
/// - Deve ser nomeado no imperativo (ex: CriarPedidoCommand)
/// - Idempotência é desejável
/// 
/// Exemplo de uso:
/// <code>
/// public record ExcluirProdutoCommand(Guid Id) : ICommand;
/// </code>
/// </remarks>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Contrato para comandos que retornam dados (ex: ID gerado).
/// </summary>
/// <typeparam name="TResponse">Tipo do dado retornado.</typeparam>
/// <remarks>
/// Variação de ICommand que retorna valor, útil para IDs gerados no banco.
/// - Implementa IRequest&lt;Result&lt;TResponse&gt;&gt; do MediatR
/// - Evite retornar grafos de objetos completos (prefira Queries para isso)
/// - Use apenas quando o retorno é estritamente necessário para o fluxo
/// 
/// Exemplo de uso:
/// <code>
/// public record CriarProdutoCommand(string Nome) : ICommand&lt;Guid&gt;;
/// </code>
/// </remarks>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
