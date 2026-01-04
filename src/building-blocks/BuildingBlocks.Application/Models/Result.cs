namespace BuildingBlocks.Application.Models;

/// <summary>
/// Padrão Result para Railway Oriented Programming
///
/// Representa o resultado de uma operação que pode ter sucesso ou falhar
/// Elimina a necessidade de exceções para controle de fluxo
/// Torna explícito o tratamento de erros
///
/// Exemplos de uso baseados no schema SQL:
///
/// Sucesso:
/// - return Result.Success(); // Comando executado com sucesso
/// - return Result.Success(produto); // Query retornou produto
///
/// Falha:
/// - return Result.Failure(Error.NotFound("PRODUTO_NAO_ENCONTRADO", "Produto não encontrado"));
/// - return Result.Failure(Error.Validation("PRECO_INVALIDO", "O preço deve ser maior que zero"));
/// - return Result.Failure(Error.Conflict("ESTOQUE_INSUFICIENTE", "Estoque insuficiente"));
///
/// Composição:
/// var resultado = await ProcessarPagamento(pedido);
/// if (resultado.IsFailure)
///     return resultado; // Propaga o erro
///
/// await AtualizarStatusPedido(pedido);
/// return Result.Success();
/// </summary>
public class Result
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação falhou
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Erro que ocorreu (None se bem-sucedido)
    /// </summary>
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Resultado de sucesso não pode ter erro");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Resultado de falha deve ter erro");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Cria um resultado de falha
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Cria um resultado de sucesso com valor
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Cria um resultado de falha com valor
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    /// <summary>
    /// Cria um resultado baseado em uma condição
    /// </summary>
    public static Result Create(bool condition, Error error) =>
        condition ? Success() : Failure(error);

    /// <summary>
    /// Cria um resultado baseado em um valor nullable
    /// </summary>
    public static Result<TValue> Create<TValue>(TValue? value, Error error) where TValue : class =>
        value is not null ? Success(value) : Failure<TValue>(error);
}

/// <summary>
/// Result com valor tipado
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Valor retornado (apenas se bem-sucedido)
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado com falha");

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Converte implicitamente um valor em Result de sucesso
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Executa uma ação se o resultado for bem-sucedido
    /// </summary>
    public Result<TValue> OnSuccess(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    /// <summary>
    /// Executa uma ação se o resultado falhou
    /// </summary>
    public Result<TValue> OnFailure(Action<Error> action)
    {
        if (IsFailure)
        {
            action(Error);
        }

        return this;
    }

    /// <summary>
    /// Mapeia o valor para outro tipo
    /// </summary>
    public Result<TOutput> Map<TOutput>(Func<TValue, TOutput> mapper)
    {
        return IsSuccess
            ? Success(mapper(Value))
            : Failure<TOutput>(Error);
    }

    /// <summary>
    /// Encadeia operações que retornam Result
    /// </summary>
    public async Task<Result<TOutput>> Bind<TOutput>(Func<TValue, Task<Result<TOutput>>> func)
    {
        return IsSuccess
            ? await func(Value)
            : Failure<TOutput>(Error);
    }

    /// <summary>
    /// Executa pattern matching no resultado
    /// </summary>
    public TOutput Match<TOutput>(
        Func<TValue, TOutput> onSuccess,
        Func<Error, TOutput> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}
