using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace AthensServiceDesk.Api.Authorization;

public sealed class ProblemDetailsAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler
        _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext httpContext,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            httpContext.Response.Headers.WWWAuthenticate =
                "Bearer";

            await WriteProblemDetailsAsync(
                httpContext,
                StatusCodes.Status401Unauthorized,
                "Authentication required",
                "A valid bearer token is required to access this resource.");

            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteProblemDetailsAsync(
                httpContext,
                StatusCodes.Status403Forbidden,
                "Access forbidden",
                "You do not have permission to perform this operation.");

            return;
        }

        await _defaultHandler.HandleAsync(
            next,
            httpContext,
            policy,
            authorizeResult);
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        string traceId =
            Activity.Current?.Id
            ?? httpContext.TraceIdentifier;

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: httpContext.Request.Path,
            extensions:
                new Dictionary<string, object?>
                {
                    ["traceId"] = traceId
                })
            .ExecuteAsync(httpContext);
    }
}