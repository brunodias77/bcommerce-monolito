namespace Bcommerce.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exceção para recurso não encontrado (HTTP 404).
/// </summary>
/// <remarks>
/// Lançada quando um recurso solicitado não existe.
/// - Busca por ID inexistente
/// - Rota ou arquivo não encontrado
/// - Mapeada automaticamente para 404 no middleware
/// 
/// Exemplo de uso:
/// <code>
/// var produto = await _repo.GetById(id);
/// if (produto == null)
///     throw new NotFoundException("Produto", id);
/// </code>
/// </remarks>
/// <param name="name">Tipo do recurso.</param>
/// <param name="key">Chave de busca.</param>
public class NotFoundException(string name, object key) 
    : ApplicationException("Não Encontrado", $"A entidade \"{name}\" ({key}) não foi encontrada.")
{
}
