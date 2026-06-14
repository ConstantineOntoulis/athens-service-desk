using System.Diagnostics;
using AthensServiceDesk.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace AthensServiceDesk.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, string title, string detail) =
            exception switch
            {
                NotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    exception.Message
                ),

                BusinessRuleException =>
                (
                    StatusCodes.Status409Conflict,
                    "Business rule violation",
                    exception.Message
                ),

                ArgumentException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    exception.Message
                ),

                _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "Unexpected server error",
                    "An unexpected error occurred while processing the request."
                )
            };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "An unexpected exception occurred while processing {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "A controlled application exception occurred while processing {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        string traceId =
            Activity.Current?.Id
            ?? httpContext.TraceIdentifier;

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: httpContext.Request.Path,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = traceId
            })
            .ExecuteAsync(httpContext);

        return true;
    }
}