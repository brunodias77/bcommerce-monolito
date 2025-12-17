using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Application.Extensions;

/// <summary>
/// Métodos de extensão para o padrão Result (Railway-Oriented Programming).
/// </summary>
/// <remarks>
/// Permite composição fluente de operações que retornam Result.
/// - Bind: Encadeia operações se o anterior for sucesso
/// - Map: Transforma o valor se sucesso
/// - Tap: Executa efeito colateral se sucesso
/// 
/// Exemplo de uso:
/// <code>
/// return await Result.Success(request)
///     .Bind(ValidarAsync)
///     .Bind(ProcessarAsync)
///     .Map(MaperResposta);
/// </code>
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Encadeia uma operação que retorna <see cref="Result"/> a um resultado existente.
    /// Se o resultado atual for falha, retorna a falha sem executar a função.
    /// </summary>
    /// <typeparam name="TIn">Tipo do valor de entrada.</typeparam>
    /// <param name="result">Resultado atual.</param>
    /// <param name="func">Função a ser executada se o resultado for sucesso.</param>
    /// <returns>O resultado da função ou a falha propagada.</returns>
    public static async Task<Result> Bind<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        return await func(result.Value);
    }

    /// <summary>
    /// Encadeia uma operação que retorna <see cref="Result{TOut}"/> a um resultado existente.
    /// Se o resultado atual for falha, retorna a falha sem executar a função.
    /// </summary>
    /// <typeparam name="TIn">Tipo do valor de entrada.</typeparam>
    /// <typeparam name="TOut">Tipo do valor de saída.</typeparam>
    /// <param name="result">Resultado atual.</param>
    /// <param name="func">Função a ser executada se o resultado for sucesso.</param>
    /// <returns>O resultado da função ou a falha propagada.</returns>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> func)
    {
        if (result.IsFailure)
        {
            return Result.Failure<TOut>(result.Error);
        }

        return await func(result.Value);
    }
}
