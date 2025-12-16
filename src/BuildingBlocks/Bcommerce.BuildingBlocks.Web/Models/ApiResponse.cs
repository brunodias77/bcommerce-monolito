namespace Bcommerce.BuildingBlocks.Web.Models;

public class ApiResponse<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public ErrorResponse? Error { get; }

    private ApiResponse(bool success, T? data, ErrorResponse? error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public static ApiResponse<T> Ok(T data) => new(true, data, null);
    public static ApiResponse<T> Failure(ErrorResponse error) => new(false, default, error);
}
