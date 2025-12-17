using Bcommerce.BuildingBlocks.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bcommerce.BuildingBlocks.Web.Filters;

/// <summary>
/// Filtro de validação de ModelState.
/// </summary>
/// <remarks>
/// Intercepta requisições inválidas antes de chegarem à Action.
/// - Formata erros de validação no padrão ValidationErrorResponse
/// - Substitui o comportamento padrão [ApiController] de validação automática
/// 
/// Exemplo de uso:
/// <code>
/// options.Filters.Add&lt;ValidationFilter&gt;();
/// </code>
/// </remarks>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new ValidationErrorResponse("Validation.Error", "Falha na validação do modelo", errors);
            
            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
