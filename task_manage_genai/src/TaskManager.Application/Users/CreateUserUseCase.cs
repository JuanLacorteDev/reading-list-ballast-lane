using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Results;
using TaskManager.Domain.Users;

namespace TaskManager.Application.Users;

public sealed record CreateUserCommand(string? Name);

/// <summary>Development/testing helper use case: creates a minimal user and returns its id.</summary>
public sealed class CreateUserUseCase(IUserRepository users)
{
    public async Task<Result<Guid>> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = User.Create(command.Name!);
        await users.AddAsync(user, cancellationToken);
        return user.Id;
    }
}
