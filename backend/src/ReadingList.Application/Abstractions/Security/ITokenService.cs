using ReadingList.Domain.Entities;

namespace ReadingList.Application.Abstractions.Security;

/// <summary>
/// Issues access tokens for authenticated users. The token format (e.g. JWT) is an Infrastructure detail.
/// </summary>
public interface ITokenService
{
    AccessToken CreateToken(User user);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);
