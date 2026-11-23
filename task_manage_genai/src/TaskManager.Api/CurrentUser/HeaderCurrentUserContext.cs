using TaskManager.Application.Abstractions;

namespace TaskManager.Api.CurrentUser;

/// <summary>
/// TEMPORARY MVP mechanism, not authentication: the current user is the id sent in the
/// X-User-Id header, validated by <see cref="RequireUserIdHeaderAttribute"/>. Replace this
/// registration with a claims-based implementation once JWT authentication exists.
/// </summary>
public sealed class HeaderCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public const string HeaderName = "X-User-Id";
    internal const string ItemKey = "TaskManager.CurrentUserId";

    public Guid UserId =>
        httpContextAccessor.HttpContext?.Items[ItemKey] is Guid userId
            ? userId
            : throw new InvalidOperationException(
                $"No validated current user. Is the endpoint decorated with [{nameof(RequireUserIdHeaderAttribute)}]?");
}
