namespace Bcommerce.BuildingBlocks.Application.Models;

/// <summary>
/// DTO de requisição para consultas paginadas.
/// </summary>
/// <remarks>
/// Encapsula parâmetros de paginação e ordenação.
/// - PageNumber inicia em 1 (normalizado)
/// - PageSize limitado a 100 (proteção de recurso)
/// - Utilizado em Queries do CQRS
/// 
/// Exemplo de uso:
/// <code>
/// public async Task&lt;IActionResult&gt; Get([FromQuery] PagedRequest request)
/// {
///     var query = new ListarProdutosQuery(request);
///     return Ok(await _mediator.Send(query));
/// }
/// </code>
/// </remarks>
/// <param name="PageNumber">Número da página (1-indexed).</param>
/// <param name="PageSize">Quantidade de itens por página (1-100).</param>
/// <param name="SortBy">Nome da propriedade para ordenação (opcional).</param>
/// <param name="IsAscending">Direção da ordenação (true = ASC, false = DESC).</param>
public record PagedRequest(int PageNumber = 1, int PageSize = 10, string? SortBy = null, bool IsAscending = true)
{
    /// <summary>Número da página normalizado (mínimo 1).</summary>
    public int PageNumber { get; init; } = PageNumber < 1 ? 1 : PageNumber;
    /// <summary>Tamanho da página normalizado (1-100, padrão 10).</summary>
    public int PageSize { get; init; } = PageSize > 100 ? 100 : (PageSize < 1 ? 10 : PageSize);
}
