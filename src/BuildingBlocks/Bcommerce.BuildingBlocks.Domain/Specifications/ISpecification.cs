using System.Linq.Expressions;

namespace Bcommerce.BuildingBlocks.Domain.Specifications;

/// <summary>
/// Contrato para o padrão Specification (DDD).
/// </summary>
/// <typeparam name="T">Tipo da entidade a ser avaliada.</typeparam>
/// <remarks>
/// Encapsula regras de consulta reutilizáveis.
/// - Pode ser convertida em Expression para uso com EF Core
/// - Combinável com And, Or, Not
/// - Usada em repositórios para filtros complexos
/// 
/// Exemplo de uso:
/// <code>
/// public class ProdutoAtivoSpec : Specification&lt;Produto&gt;
/// {
///     public override Expression&lt;Func&lt;Produto, bool&gt;&gt; ToExpression()
///         => p => p.Ativo;
/// }
/// 
/// // No repositório:
/// var spec = new ProdutoAtivoSpec();
/// var produtos = await _repo.GetListAsync(spec);
/// </code>
/// </remarks>
public interface ISpecification<T>
{
    /// <summary>Retorna a expressão lambda para uso em LINQ/EF Core.</summary>
    Expression<Func<T, bool>> ToExpression();
    /// <summary>Avalia se uma entidade satisfaz a especificação.</summary>
    bool IsSatisfiedBy(T entity);
}
