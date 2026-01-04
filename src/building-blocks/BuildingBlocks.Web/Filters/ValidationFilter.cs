using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BuildingBlocks.Web.Filters;

/// <summary>
/// Filtro para validação automática do ModelState
/// Intercepta requisições antes de chegarem ao controller e valida os dados
///
/// Este filtro trabalha em conjunto com FluentValidation e Data Annotations
/// para garantir que apenas dados válidos cheguem aos handlers
///
/// Exemplos de validações que serão capturadas (baseado no schema SQL):
///
/// Produtos:
/// - Nome do produto vazio ou nulo
/// - Preço negativo ou zero
/// - SKU duplicado ou formato inválido
/// - Estoque negativo
///
/// Pedidos:
/// - Endereço de entrega incompleto
/// - Método de pagamento inválido
/// - Valor total inconsistente
/// - Número de parcelas fora do range (1-24)
///
/// Usuários:
/// - Email em formato inválido
/// - CPF com formato incorreto
/// - Senha que não atende requisitos mínimos
///
/// Cupons:
/// - Código com menos de 3 caracteres
/// - Desconto percentual maior que 100%
/// - Data de validade inválida (fim antes do início)
///
/// Quando uma validação falha, este filtro retorna automaticamente
/// uma resposta 400 Bad Request com os erros de validação formatados
/// </summary>
public sealed class ValidationFilter : IActionFilter
{
    /// <summary>
    /// Executado antes da action do controller
    /// Valida o ModelState e retorna erro se inválido
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Se o ModelState é válido, permite que a requisição continue
        if (context.ModelState.IsValid)
        {
            return;
        }

        // Extrai todos os erros de validação do ModelState
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new
            {
                Campo = FormatFieldName(x.Key),
                Mensagem = string.IsNullOrEmpty(e.ErrorMessage)
                    ? "O campo é inválido"
                    : e.ErrorMessage
            }))
            .ToArray();

        // Cria resposta padronizada RFC 7807 com os erros de validação
        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Um ou mais erros de validação ocorreram",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Por favor, corrija os erros e tente novamente",
            Instance = context.HttpContext.Request.Path
        };

        // Adiciona os erros ao ProblemDetails
        foreach (var error in errors)
        {
            if (!problemDetails.Errors.ContainsKey(error.Campo))
            {
                problemDetails.Errors[error.Campo] = Array.Empty<string>();
            }

            var currentErrors = problemDetails.Errors[error.Campo].ToList();
            currentErrors.Add(error.Mensagem);
            problemDetails.Errors[error.Campo] = currentErrors.ToArray();
        }

        // Adiciona TraceId para rastreabilidade
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        // Adiciona código de erro customizado
        problemDetails.Extensions["code"] = "VALIDACAO_FALHOU";

        // Define o resultado como BadRequest com o ProblemDetails
        context.Result = new BadRequestObjectResult(problemDetails);
    }

    /// <summary>
    /// Executado após a action do controller
    /// Não é necessário implementar neste caso
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Não é necessário fazer nada após a execução da action
    }

    /// <summary>
    /// Formata o nome do campo para exibição
    /// Remove prefixos de binding (ex: "$." do JSON) e converte para formato amigável
    /// </summary>
    /// <param name="fieldName">Nome do campo original do ModelState</param>
    /// <returns>Nome do campo formatado</returns>
    private static string FormatFieldName(string fieldName)
    {
        // Remove prefixos comuns de binding
        if (fieldName.StartsWith("$."))
        {
            fieldName = fieldName[2..];
        }

        // Remove $ inicial se existir
        if (fieldName.StartsWith("$"))
        {
            fieldName = fieldName[1..];
        }

        // Se o campo estiver vazio após a limpeza, retorna "Geral"
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return "Geral";
        }

        // Converte primeira letra para maiúscula
        return char.ToUpper(fieldName[0]) + fieldName[1..];
    }
}
