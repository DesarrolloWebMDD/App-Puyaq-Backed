using Microsoft.AspNetCore.Diagnostics;
using Puyaq.CrossCutting.Exceptions;
using Puyaq.CrossCutting.Results;

namespace Puyaq.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var appException = exception as AppException;
        var statusCode = appException?.StatusCode
            ?? StatusCodes.Status500InternalServerError;

        var code = appException?.Code ?? "UNEXPECTED_ERROR";
        var message = appException?.Message
            ?? "Ocurrió un error inesperado.";

        if (statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Application exception {Code}. TraceId: {TraceId}",
                code,
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(
            code,
            message,
            httpContext.TraceIdentifier);

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}
