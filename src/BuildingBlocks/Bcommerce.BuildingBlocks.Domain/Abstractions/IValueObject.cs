namespace Bcommerce.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Marca uma classe como Value Object no padrão DDD.
/// </summary>
/// <remarks>
/// Value Objects são imutáveis e comparados por valor, não por identidade.
/// - Sem identidade própria (comparados por atributos)
/// - Imutáveis (qualquer mudança cria nova instância)
/// - Encapsulam validação de regras de negócio
/// 
/// Exemplo de uso:
/// <code>
/// public class Dinheiro : ValueObject
/// {
///     public decimal Valor { get; }
///     public string Moeda { get; }
///     
///     public Dinheiro(decimal valor, string moeda)
///     {
///         if (valor &lt; 0) throw new InvalidValueObjectException("Valor não pode ser negativo");
///         Valor = valor;
///         Moeda = moeda;
///     }
///     
///     protected override IEnumerable&lt;object&gt; GetEqualityComponents()
///     {
///         yield return Valor;
///         yield return Moeda;
///     }
/// }
/// </code>
/// </remarks>
public interface IValueObject
{
}
