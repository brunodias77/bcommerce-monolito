namespace Bcommerce.BuildingBlocks.Application.Models;

/// <summary>
/// Representa o resultado de uma operação que pode falhar (Railway-Oriented Programming).
/// </summary>
/// <remarks>
/// Alternativa ao lançamento de exceções para fluxo de controle.
/// - Encapsula sucesso ou falha e o erro associado
/// - Obriga a verificação de sucesso antes de acessar valor (para Result&lt;T&gt;)
/// - Facilita composição de fluxos com extensions Bind/Map
/// 
/// Exemplo de uso:
/// <code>
/// public Result&lt;int&gt; Dividir(int a, int b) {
///     if (b == 0) return Result.Failure&lt;int&gt;(Error.Validation("DivZero", "B é zero"));
///     return Result.Success(a / b);
/// }
/// </code>
/// </remarks>
public class Result
{
    /// <summary>Indica se a operação foi bem-sucedida.</summary>
    public bool IsSuccess { get; }
    /// <summary>Indica se a operação falhou (oposto de IsSuccess).</summary>
    public bool IsFailure => !IsSuccess;
    /// <summary>Erro associado (Error.None se sucesso).</summary>
    public Error Error { get; }

    /// <summary>Construtor protegido para garantir consistência estado/erro.</summary>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Cria um resultado de sucesso sem valor.</summary>
    public static Result Success() => new(true, Error.None);
    /// <summary>Cria um resultado de falha com o erro especificado.</summary>
    public static Result Failure(Error error) => new(false, error);
    /// <summary>Cria um resultado de sucesso com valor.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    /// <summary>Cria um resultado de falha tipado com o erro especificado.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Resultado tipado que carrega um valor em caso de sucesso.
/// </summary>
/// <typeparam name="TValue">Tipo do valor retornado.</typeparam>
/// <remarks>
/// Acessar <see cref="Value"/> em um resultado de falha lança InvalidOperationException.
/// Sempre verifique <see cref="Result.IsSuccess"/> antes de acessar o valor.
/// </remarks>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Valor do resultado (lança exceção se acessado em resultado de falha).
    /// </summary>
    /// <exception cref="InvalidOperationException">Quando acessado em resultado de falha.</exception>
    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado de falha.");

    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Conversão implícita de valor para Result&lt;TValue&gt;.
    /// Valores nulos são convertidos em falha com Error.NullValue.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}
