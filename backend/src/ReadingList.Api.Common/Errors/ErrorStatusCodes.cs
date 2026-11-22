using Microsoft.AspNetCore.Http;
using ReadingList.Application.Common.Results;

namespace ReadingList.Api.Common.Errors;

/// <summary>
/// Maps Application error categories to HTTP status codes. This is the only place that knows about both.
/// </summary>
public static class ErrorStatusCodes
{
    public static int From(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };
}
