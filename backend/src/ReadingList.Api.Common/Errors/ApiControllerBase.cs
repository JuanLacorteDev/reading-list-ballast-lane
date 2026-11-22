using Microsoft.AspNetCore.Mvc;
using ReadingList.Application.Common.Results;

namespace ReadingList.Api.Common.Errors;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Turns an expected failure into an RFC 9457 ProblemDetails response. The error code goes in "code"
    /// so the frontend can react to it without parsing the message.
    /// </summary>
    protected IActionResult Problem(Error error) => Problem(
        detail: error.Message,
        statusCode: ErrorStatusCodes.From(error.Type),
        title: error.Code,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
