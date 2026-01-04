namespace BuildingBlocks.Application.Models;

/// <summary>
/// Representa um erro padronizado no sistema
/// Usado em conjunto com o padrão Result para Railway Oriented Programming
///
/// Exemplos de erros baseados no schema SQL:
///
/// Validação:
/// - "PRODUTO_PRECO_INVALIDO": "O preço do produto deve ser maior ou igual a zero"
/// - "ESTOQUE_INSUFICIENTE": "Estoque insuficiente para a quantidade solicitada"
/// - "SKU_DUPLICADO": "Já existe um produto com este SKU"
///
/// Regras de Negócio:
/// - "CUPOM_EXPIRADO": "O cupom informado está expirado"
/// - "COMPRA_MINIMA_NAO_ATINGIDA": "O valor mínimo de compra não foi atingido"
/// - "LIMITE_PARCELAS_EXCEDIDO": "O número de parcelas deve estar entre 1 e 24"
///
/// Não Encontrado:
/// - "PRODUTO_NAO_ENCONTRADO": "Produto não encontrado"
/// - "PEDIDO_NAO_ENCONTRADO": "Pedido não encontrado"
/// - "USUARIO_NAO_ENCONTRADO": "Usuário não encontrado"
///
/// Estado Inválido:
/// - "PEDIDO_JA_PAGO": "O pedido já foi pago e não pode ser modificado"
/// - "CARRINHO_JA_CONVERTIDO": "O carrinho já foi convertido em pedido"
/// - "PAGAMENTO_JA_PROCESSADO": "O pagamento já foi processado"
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Código único do erro (para identificação programática)
    /// Convenção: MAIUSCULO_COM_UNDERSCORES
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Mensagem descritiva do erro (para exibição ao usuário)
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Tipo do erro (para categorização)
    /// </summary>
    public ErrorType Type { get; init; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    /// <summary>
    /// Erro padrão para indicar que nenhum erro ocorreu
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>
    /// Erro padrão para falhas inesperadas
    /// </summary>
    public static readonly Error NullValue = new(
        "ERRO_VALOR_NULO",
        "Um valor nulo foi fornecido",
        ErrorType.Validation);

    /// <summary>
    /// Cria um erro de validação
    /// </summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    /// <summary>
    /// Cria um erro de recurso não encontrado
    /// </summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Cria um erro de conflito (violação de regra de negócio)
    /// </summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Cria um erro de falha (erro interno do sistema)
    /// </summary>
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    /// <summary>
    /// Cria um erro de não autorizado
    /// </summary>
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Cria um erro de proibido (sem permissão)
    /// </summary>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);
}

/// <summary>
/// Tipos de erro do sistema
/// Mapeiam diretamente para códigos HTTP
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Nenhum erro
    /// </summary>
    None = 0,

    /// <summary>
    /// Erro de validação (HTTP 400 Bad Request)
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Recurso não encontrado (HTTP 404 Not Found)
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Conflito / Regra de negócio violada (HTTP 409 Conflict)
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// Falha interna (HTTP 500 Internal Server Error)
    /// </summary>
    Failure = 4,

    /// <summary>
    /// Não autenticado (HTTP 401 Unauthorized)
    /// </summary>
    Unauthorized = 5,

    /// <summary>
    /// Sem permissão (HTTP 403 Forbidden)
    /// </summary>
    Forbidden = 6
}
