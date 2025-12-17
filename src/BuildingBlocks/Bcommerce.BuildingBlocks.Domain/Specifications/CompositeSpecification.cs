// Esta classe já foi implementada implicitamente dentro de Specification.cs 
// através das classes internas AndSpecification, OrSpecification e NotSpecification
// mas vou criar o arquivo para manter a estrutura de pastas solicitada

namespace Bcommerce.BuildingBlocks.Domain.Specifications;

// Classe placeholder para manter a estrutura de arquivos solicitada
// As implementações reais de composição estão em Specification.cs
/// <summary>
/// Especificação composta placeholder.
/// </summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
/// <remarks>
/// Classe mantida para compatibilidade de estrutura de pastas.
/// - Implementações reais de composição estão em Specification.cs
/// - AndSpecification, OrSpecification e NotSpecification são internas
/// - Use Specification&lt;T&gt;.And(), .Or(), .Not() diretamente
/// 
/// Exemplo de uso:
/// <code>
/// // Prefira usar:
/// var spec = new MinhaSpec().And(new OutraSpec());
/// 
/// // Em vez de herdar CompositeSpecification
/// </code>
/// </remarks>
public class CompositeSpecification<T> : Specification<T>
{
    public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
    {
        return x => true;
    }
}
