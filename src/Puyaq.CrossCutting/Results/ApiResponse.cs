namespace Puyaq.CrossCutting.Results;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    ApiError? Error,
    string? TraceId)
{
    public static ApiResponse<T> Ok(
        T data,
        string? message = null,
        string? traceId = null) =>
        new(true, data, message, null, traceId);

    public static ApiResponse<T> Fail(
        string code,
        string message,
        string? traceId = null) =>
        new(false, default, null, new ApiError(code, message), traceId);
}

public sealed record ApiError(string Code, string Message);
