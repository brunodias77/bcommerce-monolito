using Bcommerce.BuildingBlocks.Application.Models;
using MediatR;

namespace Bcommerce.BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Contrato para manipuladores de comandos sem retorno (void).
/// </summary>
/// <typeparam name="TCommand">Tipo do comando a ser manipulado.</typeparam>
/// <remarks>
/// Encapsula a lógica de execução de um comando específico.
/// - Processa a regra de negócio
/// - Persiste alterações via Repositório/UnitOfWork
/// - Retorna Result indicando sucesso ou falha
/// 
/// Exemplo de uso:
/// <code>
/// public class ExcluirProdutoHandler : ICommandHandler&lt;ExcluirProdutoCommand&gt;
/// {
///     public async Task&lt;Result&gt; Handle(ExcluirProdutoCommand request, CancellationToken ct)
///     {
///         // Lógica de exclusão...
///         return Result.Success();
///     }
/// }
/// </code>
/// </remarks>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Contrato para manipuladores de comandos com retorno.
/// </summary>
/// <typeparam name="TCommand">Tipo do comando.</typeparam>
/// <typeparam name="TResponse">Tipo da resposta.</typeparam>
/// <remarks>
/// Versão tipada do Handler para comandos que produzem resultado.
/// - Segue o padrão Request-Response do MediatR
/// - Deve tratar exceções de domínio e converter em Result falho
/// 
/// Exemplo de uso:
/// <code>
/// public class CriarProdutoHandler : ICommandHandler&lt;CriarProdutoCommand, Guid&gt;
/// {
///     public async Task&lt;Result&lt;Guid&gt;&gt; Handle(CriarProdutoCommand cmd, CancellationToken ct)
///     {
///         var id = await _service.Criar(cmd);
///         return Result.Success(id);
///     }
/// }
/// </code>
/// </remarks>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
