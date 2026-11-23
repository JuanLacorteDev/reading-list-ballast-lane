using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TaskManager.Application.Common.Results;

namespace TaskManager.Api.Errors;

/// <summary>
/// Builds the RFC 9457 ProblemDetails used for every error response:
/// <c>title = code</c>, <c>detail = message</c>, <c>status</c>, and extension <c>code</c>.
/// </summary>
public static class ErrorProblemDetails
{
    public static ProblemDetails Create(HttpContext httpContext, string code, string message, int status)
    {
        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateProblemDetails(httpContext, statusCode: status, title: code, detail: message);
        problem.Extensions["code"] = code;
        return problem;
    }

    public static IActionResult ToActionResult(HttpContext httpContext, Error error)
    {
        var status = ErrorStatusCodes.From(error.Type);
        return new ObjectResult(Create(httpContext, error.Code, error.Message, status))
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}
