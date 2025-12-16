namespace BuildingBlocks.Application.Results;


/// <summary>
/// Representa um erro no padrão Result.
/// </summary>
/// <remarks>
/// Separa código de erro (para lógica) de mensagem (para UI/logs).
/// 
/// Como usar:
/// 1. Defina erros estáticos nas entidades ou use Error.Validation/NotFound
/// 2. Retorne Result.Fail(Error) em vez de lançar exceções para erros de domínio
/// 
/// Exemplo:
/// <code>
/// public static class DomainErrors
/// {
///     public static readonly Error InvalidEmail = Error.Validation("User.InvalidEmail", "Email format is invalid");
/// }
/// </code>
/// </remarks>
public sealed record Error
{
    /// <summary>
    /// Código único do erro (ex: "PRODUCT_NOT_FOUND", "INSUFFICIENT_STOCK").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Mensagem descritiva do erro.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Tipo do erro (Validation, NotFound, Conflict, etc.).
    /// </summary>
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    /// <summary>
    /// Erro de validação (400 Bad Request).
    /// </summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    /// <summary>
    /// Recurso não encontrado (404 Not Found).
    /// </summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Conflito de negócio (409 Conflict).
    /// </summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Falha genérica (500 Internal Server Error).
    /// </summary>
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    /// <summary>
    /// Não autorizado (401 Unauthorized).
    /// </summary>
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Acesso negado (403 Forbidden).
    /// </summary>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Erro nenhum (sucesso).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static implicit operator string(Error error) => error.Code;
}

/// <summary>
/// Tipos de erro para categorização.
/// </summary>
public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Failure,
    Unauthorized,
    Forbidden
}

/// <summary>
/// Extensões para facilitar criação de erros comuns.
/// </summary>
public static class ErrorExtensions
{
    public static Error ProductNotFound(Guid productId) =>
        Error.NotFound("PRODUCT_NOT_FOUND", $"Product with ID {productId} was not found");

    public static Error OrderNotFound(Guid orderId) =>
        Error.NotFound("ORDER_NOT_FOUND", $"Order with ID {orderId} was not found");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("USER_NOT_FOUND", $"User with ID {userId} was not found");

    public static Error InsufficientStock(int available, int requested) =>
        Error.Conflict("INSUFFICIENT_STOCK", $"Insufficient stock. Available: {available}, Requested: {requested}");

    public static Error InvalidCoupon(string reason) =>
        Error.Validation("INVALID_COUPON", reason);

    public static Error InvalidStatus(string currentStatus, string operation) =>
        Error.Conflict("INVALID_STATUS", $"Cannot {operation} when status is {currentStatus}");
}