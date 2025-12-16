using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BuildingBlocks.Presentation.Filters;

/// <summary>
/// Filtro para validação automática de ModelState.
/// </summary>
/// <remarks>
/// Este filtro:
/// 1. Verifica ModelState antes da execução do action
/// 2. Retorna 400 Bad Request com ValidationProblemDetails se inválido
/// 3. Evita código boilerplate de validação nos controllers
/// 
/// NOTA: O [ApiController] já faz validação automática.
/// Este filtro é útil quando você quer customizar a resposta.
/// 
/// Registro:
/// <code>
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ValidationFilter&gt;();
/// });
/// </code>
/// 
/// Ou desabilitando validação automática do [ApiController]:
/// <code>
/// builder.Services.Configure&lt;ApiBehaviorOptions&gt;(options =>
/// {
///     options.SuppressModelStateInvalidFilter = true;
/// });
/// </code>
/// </remarks>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Falha na Validação",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Ocorreu um ou mais erros de validação.",
                Instance = context.HttpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = "VALIDATION_ERROR";
            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nada a fazer após a execução
    }
}

/// <summary>
/// Filtro de validação assíncrono com suporte a validação customizada.
/// </summary>
public class AsyncValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var problemDetails = CreateValidationProblemDetails(context);
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        // Executa a action
        await next();
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        ActionExecutingContext context)
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Any() == true)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e =>
                    string.IsNullOrEmpty(e.ErrorMessage)
                        ? e.Exception?.Message ?? "Valor inválido"
                        : e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Falha na Validação",
            Status = StatusCodes.Status400BadRequest,
            Detail = $"{errors.Sum(e => e.Value.Length)} erros de validação ocorreram.",
            Instance = context.HttpContext.Request.Path,
            Extensions =
            {
                ["errorCode"] = "VALIDATION_ERROR",
                ["traceId"] = context.HttpContext.TraceIdentifier
            }
        };
    }
}
