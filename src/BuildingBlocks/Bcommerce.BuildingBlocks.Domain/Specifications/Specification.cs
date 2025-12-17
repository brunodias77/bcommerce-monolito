using System.Linq.Expressions;

namespace Bcommerce.BuildingBlocks.Domain.Specifications;

/// <summary>
/// Classe base abstrata para implementação do padrão Specification.
/// </summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
/// <remarks>
/// Fornece combinação de especificações com operações lógicas.
/// - And(): combina com E lógico
/// - Or(): combina com OU lógico
/// - Not(): nega a especificação
/// 
/// Exemplo de uso:
/// <code>
/// var ativoEEmEstoque = new ProdutoAtivoSpec()
///     .And(new ProdutoEmEstoqueSpec());
/// 
/// var produtos = await _repo.GetListAsync(ativoEEmEstoque);
/// </code>
/// </remarks>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>Retorna a expressão que define o critério da especificação.</summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc />
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>Combina com outra especificação usando AND.</summary>
    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    /// <summary>Combina com outra especificação usando OR.</summary>
    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    /// <summary>Retorna a negação desta especificação.</summary>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

internal class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    private readonly Specification<T> _left = left;
    private readonly Specification<T> _right = right;

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

internal class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    private readonly Specification<T> _left = left;
    private readonly Specification<T> _right = right;

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

internal class NotSpecification<T>(Specification<T> specification) : Specification<T>
{
    private readonly Specification<T> _specification = specification;

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
