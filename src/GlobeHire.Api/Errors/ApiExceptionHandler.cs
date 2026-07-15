using GlobeHire.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GlobeHire.Api.Errors;

public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly Action<ILogger, string, string, Exception?> LogBusinessRuleRejected =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1001, nameof(LogBusinessRuleRejected)),
            "Business rule {BusinessRuleCode} rejected request {TraceId}");

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleViolationException businessException)
        {
            return false;
        }

        LogBusinessRuleRejected(
            logger,
            businessException.Code,
            httpContext.TraceIdentifier,
            null);

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Business rule violation",
                Detail = businessException.Message,
                Type = "https://globehire.example/problems/business-rule-violation",
                Extensions =
                {
                    ["code"] = businessException.Code,
                    ["traceId"] = httpContext.TraceIdentifier,
                },
            },
            Exception = exception,
        }).ConfigureAwait(false);
    }
}
