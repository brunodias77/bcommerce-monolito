namespace Bcommerce.BuildingBlocks.Web.Models;

public record ValidationErrorResponse(string Code, string Message, IDictionary<string, string[]> Errors) 
    : ErrorResponse(Code, Message);
