using System.Security.Claims;

namespace ReadingList.Api.Common.Authentication;

public static class ClaimsPrincipalExtensions
{
    private const string SubjectClaim = "sub";

    /// <summary>
    /// Reads the user id from the "sub" claim. Only call it on endpoints protected by [Authorize].
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(SubjectClaim);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("The authenticated user has no valid 'sub' claim.");
    }
}
