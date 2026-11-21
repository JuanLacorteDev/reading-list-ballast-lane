using ReadingList.Application.Abstractions.Security;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Auth;

public sealed record RegisterUserRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt)
{
    public static AuthResponse FromAccessToken(AccessToken token) => new(token.Token, token.ExpiresAt);
}

public sealed record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt)
{
    public static UserResponse FromDomain(User user) => new(
        user.Id,
        user.Name,
        user.Email.Value,
        user.CreatedAt);
}
