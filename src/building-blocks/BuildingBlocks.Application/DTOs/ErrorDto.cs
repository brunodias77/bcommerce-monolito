using BuildingBlocks.Application.Models;

namespace BuildingBlocks.Application.DTOs;

/// <summary>
/// DTO para serialização de erros em respostas HTTP
/// Usado pela camada de apresentação (API) para retornar erros padronizados
///
/// Formato de resposta JSON:
/// {
///   "code": "PRODUTO_NAO_ENCONTRADO",
///   "message": "Produto com ID '123' não foi encontrado",
///   "type": "NotFound"
/// }
///
/// Exemplos de erros retornados pela API baseados no schema SQL:
///
/// 400 Bad Request (Validation):
/// - "PRECO_INVALIDO": "O preço deve ser maior ou igual a zero"
/// - "QUANTIDADE_INVALIDA": "A quantidade deve ser maior que zero"
/// - "EMAIL_INVALIDO": "O formato do e-mail é inválido"
/// - "CPF_INVALIDO": "O CPF informado é inválido"
///
/// 404 Not Found:
/// - "PRODUTO_NAO_ENCONTRADO": "Produto não encontrado"
/// - "PEDIDO_NAO_ENCONTRADO": "Pedido não encontrado"
/// - "USUARIO_NAO_ENCONTRADO": "Usuário não encontrado"
/// - "CUPOM_NAO_ENCONTRADO": "Cupom não encontrado"
///
/// 409 Conflict (Business Rule):
/// - "SKU_DUPLICADO": "Já existe um produto com este SKU"
/// - "ESTOQUE_INSUFICIENTE": "Estoque insuficiente para completar a operação"
/// - "CUPOM_EXPIRADO": "O cupom está expirado"
/// - "PEDIDO_JA_PAGO": "O pedido já foi pago"
/// - "LIMITE_PARCELAS_EXCEDIDO": "Número de parcelas deve estar entre 1 e 24"
///
/// 500 Internal Server Error (Failure):
/// - "ERRO_PROCESSAMENTO_PAGAMENTO": "Erro ao processar pagamento"
/// - "ERRO_INTEGRACAO_GATEWAY": "Erro ao comunicar com gateway de pagamento"
/// </summary>
public sealed record ErrorDto
{
    /// <summary>
    /// Código do erro
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Mensagem descritiva
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Tipo do erro (Validation, NotFound, Conflict, etc.)
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Cria um ErrorDto a partir de um Error
    /// </summary>
    public static ErrorDto FromError(Error error)
    {
        return new ErrorDto
        {
            Code = error.Code,
            Message = error.Message,
            Type = error.Type.ToString()
        };
    }

    /// <summary>
    /// Cria um ErrorDto diretamente
    /// </summary>
    public static ErrorDto Create(string code, string message, string type)
    {
        return new ErrorDto
        {
            Code = code,
            Message = message,
            Type = type
        };
    }
}
