using TaskManager.Application.Common.Results;

namespace TaskManager.Api.Errors;

/// <summary>The single place where application error types are mapped to HTTP status codes.</summary>
public static class ErrorStatusCodes
{
    public static int From(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "No HTTP status mapped for this error type."),
    };
}
