using ReadingList.Application.Common.Results;

namespace ReadingList.Application.Auth;

/// <summary>
/// Registration, login and current-user use cases.
/// </summary>
public interface IAuthService
{
    Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
