using Bcommerce.BuildingBlocks.Application.Exceptions;
using Bcommerce.BuildingBlocks.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Bcommerce.BuildingBlocks.Web.Filters;

public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Exceção capturada pelo filtro de API");

        var response = context.Exception switch
        {
            ValidationException validationEx => 
                new ValidationErrorResponse("Validation.Error", "Erro de validação", validationEx.Errors),
            
            NotFoundException notFoundEx =>
                new ErrorResponse("Resource.NotFound", notFoundEx.Message, null, Application.Models.ErrorType.NotFound),
            
            UnauthorizedException unauthorizedEx =>
                new ErrorResponse("Auth.Unauthorized", unauthorizedEx.Message, null, Application.Models.ErrorType.Unauthorized),

            Application.Exceptions.ApplicationException appEx => 
                new ErrorResponse("Application.Error", appEx.Message),

            _ => new ErrorResponse("Server.Error", "Ocorreu um erro interno no servidor.")
        };

        context.Result = context.Exception switch
        {
            ValidationException => new BadRequestObjectResult(response),
            NotFoundException => new NotFoundObjectResult(response),
            UnauthorizedException => new UnauthorizedObjectResult(response),
            Application.Exceptions.ApplicationException => new BadRequestObjectResult(response),
            _ => new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError }
        };

        context.ExceptionHandled = true;
    }
}
