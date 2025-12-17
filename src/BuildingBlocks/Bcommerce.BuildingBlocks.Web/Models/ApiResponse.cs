namespace Bcommerce.BuildingBlocks.Web.Models;

/// <summary>
/// Wrapper padrão para respostas da API.
/// </summary>
/// <typeparam name="T">Tipo do dado retornado.</typeparam>
/// <remarks>
/// Unifica a estrutura de respostas de sucesso e erro.
/// - Facilita o consumo pelo frontend
/// - Encapsula dados e metadados de erro
/// 
/// Exemplo de uso:
/// <code>
/// return ApiResponse&lt;UserDto&gt;.Ok(user);
/// </code>
/// </remarks>
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
