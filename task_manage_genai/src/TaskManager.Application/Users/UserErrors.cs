using TaskManager.Application.Common.Results;

namespace TaskManager.Application.Users;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound("User.NotFound", "The current user does not exist.");
}
