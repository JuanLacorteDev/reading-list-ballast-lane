using Microsoft.AspNetCore.Diagnostics;
using TaskManager.Domain.Common;

namespace TaskManager.Api.Errors;

/// <summary>Maps a broken business rule (<see cref="DomainException"/>) to HTTP 400 ProblemDetails.</summary>
public sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = ErrorProblemDetails.Create(
                httpContext, domainException.Code, domainException.Message, StatusCodes.Status400BadRequest),
        });
    }
}
