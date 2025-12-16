namespace BuildingBlocks.Application.Results;

/// <summary>
/// Representa o resultado de uma operação sem valor de retorno.
/// Implementa o padrão "Railway Oriented Programming" para tratamento de erros funcional.
/// </summary>
/// <remarks>
/// Características:
/// - Explicitamente define Sucesso ou Falha
/// - Evita o uso de Exceptions para controle de fluxo (try/catch custoso)
/// - Permite composição functional (OnSuccess, OnFailure)
/// 
/// Versão específica para Application Layer que usa Error tipado.
/// </remarks>
public class Result
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação falhou.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Erro ocorrido (null se sucesso).
    /// </summary>
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Cria um resultado de sucesso.
    /// </summary>
    public static Result Ok() => new(true, Error.None);

    /// <summary>
    /// Cria um resultado de falha.
    /// </summary>
    public static Result Fail(Error error) => new(false, error);

    /// <summary>
    /// Cria um resultado de sucesso com valor.
    /// </summary>
    public static Result<T> Ok<T>(T value) => new(value, true, Error.None);

    /// <summary>
    /// Cria um resultado de falha com valor.
    /// </summary>
    public static Result<T> Fail<T>(Error error) => new(default, false, error);

    /// <summary>
    /// Combina múltiplos resultados (falha se qualquer um falhar).
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Ok();
    }

    /// <summary>
    /// Executa ação se resultado for sucesso.
    /// </summary>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
            action();

        return this;
    }

    /// <summary>
    /// Executa ação se resultado for falha.
    /// </summary>
    public Result OnFailure(Action<Error> action)
    {
        if (IsFailure)
            action(Error);

        return this;
    }

    /// <summary>
    /// Lança exceção se falha (útil em cenários onde falha é excepcional).
    /// </summary>
    public void ThrowIfFailure()
    {
        if (IsFailure)
            throw new InvalidOperationException($"[{Error.Code}] {Error.Message}");
    }
}

/// <summary>
/// Representa o resultado de uma operação com valor de retorno.
/// Suporta operações monádicas (Map, Bind) para encadeamento.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Valor retornado pela operação (default se falha).
    /// </summary>
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Mapeia o valor para outro tipo.
    /// </summary>
    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (IsFailure)
            return Fail<TResult>(Error);

        return Ok(mapper(Value!));
    }

    /// <summary>
    /// Mapeia o valor para outro Result.
    /// </summary>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
    {
        if (IsFailure)
            return Fail<TResult>(Error);

        return binder(Value!);
    }

    /// <summary>
    /// Executa ação se sucesso.
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Value != null)
            action(Value);

        return this;
    }

    /// <summary>
    /// Executa ação se falha.
    /// </summary>
    public new Result<T> OnFailure(Action<Error> action)
    {
        if (IsFailure)
            action(Error);

        return this;
    }

    /// <summary>
    /// Obtém o valor ou lança exceção.
    /// </summary>
    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException($"[{Error.Code}] {Error.Message}");

        return Value!;
    }

    /// <summary>
    /// Obtém o valor ou retorna valor padrão.
    /// </summary>
    public T? GetValueOrDefault(T? defaultValue = default)
    {
        return IsSuccess ? Value : defaultValue;
    }

    /// <summary>
    /// Tenta obter o valor.
    /// </summary>
    public bool TryGetValue(out T? value)
    {
        value = IsSuccess ? Value : default;
        return IsSuccess;
    }

    /// <summary>
    /// Conversão implícita de valor para Result<T>.
    /// </summary>
    public static implicit operator Result<T>(T value) => Ok(value);

    /// <summary>
    /// Conversão implícita de Error para Result<T>.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Fail<T>(error);
}



