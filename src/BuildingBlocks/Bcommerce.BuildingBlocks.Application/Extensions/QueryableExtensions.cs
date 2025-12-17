using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Application.Extensions;

/// <summary>
/// Métodos de extensão para <see cref="IQueryable{T}"/>.
/// Fornece utilitários para paginação e transformação de consultas.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converte um <see cref="IQueryable{T}"/> em uma lista paginada de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo dos elementos da consulta.</typeparam>
    /// <param name="source">Consulta origem.</param>
    /// <param name="pageNumber">Número da página (começa em 1).</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <returns>Uma <see cref="PaginatedList{T}"/> contendo os itens da página solicitada.</returns>
    /// <remarks>
    /// Aplica paginação diretamente na consulta (IQueryable).
    /// - Executa Skip() e Take() no banco de dados
    /// - Retorna total de itens e páginas
    /// - Materializa a consulta apenas para a página atual
    /// 
    /// Exemplo de uso:
    /// <code>
    /// var pagina = await _context.Produtos
    ///     .Where(p => p.Ativo)
    ///     .ToPaginatedListAsync(pageNumber: 1, pageSize: 10);
    /// </code>
    /// </remarks>
    public static Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        return PaginatedList<T>.CreateAsync(source, pageNumber, pageSize);
    }
}
