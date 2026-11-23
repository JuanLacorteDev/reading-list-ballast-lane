using Microsoft.AspNetCore.Mvc.Filters;
using TaskManager.Api.Errors;
using TaskManager.Application.Common.Results;

namespace TaskManager.Api.CurrentUser;

public static class UserContextErrors
{
    public static readonly Error MissingUserId = Error.Validation(
        "UserContext.MissingUserId", $"The {HeaderCurrentUserContext.HeaderName} header is required.");

    public static readonly Error InvalidUserId = Error.Validation(
        "UserContext.InvalidUserId", $"The {HeaderCurrentUserContext.HeaderName} header must be a non-empty GUID.");
}

/// <summary>
/// Validates the X-User-Id header before the action runs (and before body validation),
/// rejecting a missing or malformed value with 400. On success the parsed id is made
/// available to <see cref="HeaderCurrentUserContext"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireUserIdHeaderAttribute : ActionFilterAttribute
{
    public RequireUserIdHeaderAttribute() => Order = int.MinValue;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var raw = httpContext.Request.Headers[HeaderCurrentUserContext.HeaderName].ToString();

        Error? error = null;
        if (string.IsNullOrWhiteSpace(raw))
            error = UserContextErrors.MissingUserId;
        else if (!Guid.TryParse(raw, out var userId) || userId == Guid.Empty)
            error = UserContextErrors.InvalidUserId;
        else
            httpContext.Items[HeaderCurrentUserContext.ItemKey] = userId;

        if (error is not null)
            context.Result = ErrorProblemDetails.ToActionResult(httpContext, error);
    }
}
