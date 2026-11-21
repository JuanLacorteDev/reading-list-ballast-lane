using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Application.Abstractions.Persistence;

/// <summary>
/// Persistence port for users.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
