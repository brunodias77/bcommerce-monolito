using Bcommerce.BuildingBlocks.Application.Models;

namespace Bcommerce.BuildingBlocks.Web.Models;

public record ErrorResponse(string Code, string Message, string? Details = null, ErrorType Type = ErrorType.Failure)
{
    public static ErrorResponse FromError(Error error) => new(error.Code, error.Description, null, error.Type);
}
