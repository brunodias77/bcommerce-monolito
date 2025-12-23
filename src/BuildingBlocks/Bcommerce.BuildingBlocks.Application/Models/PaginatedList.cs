namespace Bcommerce.BuildingBlocks.Application.Models;

/// <summary>
/// Representa uma lista paginada de itens com metadados de navegação.
/// </summary>
/// <typeparam name="T">Tipo dos itens na lista.</typeparam>
/// <remarks>
/// Retorno padrão para consultas de listagem.
/// - Contém metadados: TotalPages, TotalCount, HasNextPage
/// - Deve ser gerado via CreateAsync ou extensão IQueryable
/// - Otimiza tráfego de rede enviando apenas dados da página
/// 
/// Exemplo de uso:
/// <code>
/// var paginacao = await _context.Produtos
///     .ToPaginatedListAsync(1, 10);
/// </code>
/// </remarks>
public class PaginatedList<T>
{
    /// <summary>Itens da página atual (somente leitura).</summary>
    public IReadOnlyList<T> Items { get; }
    /// <summary>Número da página atual (1-indexed).</summary>
    public int PageNumber { get; }
    /// <summary>Quantidade de itens por página.</summary>
    public int PageSize { get; }
    /// <summary>Total de itens em todas as páginas.</summary>
    public int TotalCount { get; }
    /// <summary>Total de páginas disponíveis.</summary>
    public int TotalPages { get; }
    /// <summary>Indica se existe página anterior.</summary>
    public bool HasPreviousPage => PageNumber > 1;
    /// <summary>Indica se existe próxima página.</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Cria uma nova instância de lista paginada.
    /// </summary>
    /// <param name="items">Itens da página atual.</param>
    /// <param name="count">Total de itens (todas as páginas).</param>
    /// <param name="pageNumber">Número da página atual.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    public PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }

    /// <summary>
    /// Cria uma lista paginada a partir de um IQueryable de forma assíncrona.
    /// </summary>
    /// <param name="source">Consulta origem.</param>
    /// <param name="pageNumber">Número da página desejada.</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <returns>Lista paginada com os itens da página solicitada.</returns>
    /// <remarks>Para uso com EF Core, substitua por implementação com ToListAsync.</remarks>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        // Nota: Em uma implementação real com EF Core, usaria ToListAsync.
        // Como aqui é BuildingBlocks e não queremos depedência direta do EF Core,
        // vamos assumir que o IQueryable pode ser materializado ou será tratado na infra.
        // Para fins deste arquivo, faremos a lógica básica.
        
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
