using BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Extensões para converter Result em IActionResult.
/// </summary>
/// <remarks>
/// Use quando não herdar de ApiControllerBase ou em minimal APIs.
/// 
/// Minimal API:
/// <code>
/// app.MapGet("/users/{id}", async (Guid id, IMediator mediator) =>
/// {
///     var result = await mediator.Send(new GetUserByIdQuery(id));
///     return result.ToActionResult();
/// });
/// </code>
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Converte Result em IResult para Minimal APIs.
    /// </summary>
    public static IResult ToResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return result.Error.ToProblemResult();
    }

    /// <summary>
    /// Converte Result&lt;T&gt; em IResult para Minimal APIs.
    /// </summary>
    public static IResult ToResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return result.Error.ToProblemResult();
    }

    /// <summary>
    /// Converte Result&lt;T&gt; em IResult com Created para Minimal APIs.
    /// </summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, string uri)
    {
        if (result.IsSuccess)
            return Results.Created(uri, result.Value);

        return result.Error.ToProblemResult();
    }

    /// <summary>
    /// Converte Error em Problem IResult.
    /// </summary>
    public static IResult ToProblemResult(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: GetErrorTitle(error.Type),
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code
            });
    }

    /// <summary>
    /// Match pattern - executa função baseado no estado do Result.
    /// </summary>
    public static TResult Match<TResult>(
        this Result result,
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }

    /// <summary>
    /// Match pattern - executa função baseado no estado do Result&lt;T&gt;.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess && result.Value != null
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    private static string GetErrorTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Resource Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Failure => "Internal Server Error",
        _ => "An error occurred"
    };
}
