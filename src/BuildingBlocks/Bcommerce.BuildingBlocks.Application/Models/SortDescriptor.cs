namespace Bcommerce.BuildingBlocks.Application.Models;

/// <summary>
/// Descritor de ordenação para consultas dinâmicas.
/// </summary>
/// <remarks>
/// Define por qual campo e direção uma lista deve ser ordenada.
/// - Field: Nome da propriedade (Case-insensitive em muitos executores)
/// - IsAscending: True para crescente, False para decrescente
/// - Segurança: Validar se Field existe na entidade para evitar SQL Injection dinâmico
/// 
/// Exemplo de uso:
/// <code>
/// query = sort.IsAscending 
///     ? query.OrderBy(sort.Field) 
///     : query.OrderByDescending(sort.Field);
/// </code>
/// </remarks>
/// <param name="Field">Nome da propriedade para ordenação.</param>
/// <param name="IsAscending">True para ASC, False para DESC.</param>
public record SortDescriptor(string Field, bool IsAscending);
