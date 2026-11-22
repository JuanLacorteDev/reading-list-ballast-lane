using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReadingList.Domain.Common.Exceptions;

namespace ReadingList.Api.Common.Errors;

/// <summary>
/// A <see cref="DomainException"/> means the request broke a business rule, so it is the client's fault: 400.
/// Any other exception falls through to the default 500 ProblemDetails.
/// </summary>
public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
            return false;

        logger.LogInformation("Business rule violated: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain.RuleViolation",
                Detail = exception.Message,
                Extensions = { ["code"] = "Domain.RuleViolation" },
            },
        });
    }
}
