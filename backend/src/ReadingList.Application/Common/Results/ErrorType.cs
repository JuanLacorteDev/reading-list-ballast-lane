namespace ReadingList.Application.Common.Results;

/// <summary>
/// Error categories. The API layer maps each one to an HTTP status code.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}
