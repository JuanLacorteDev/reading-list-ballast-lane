namespace TaskManager.Application.Abstractions;

/// <summary>
/// Identifies the user on whose behalf a use case runs. The MVP implementation reads the
/// X-User-Id header in the API layer; a JWT-based implementation can replace it without
/// touching the use cases.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>The current user's id. Implementations guarantee a valid, non-empty GUID.</summary>
    Guid UserId { get; }
}
