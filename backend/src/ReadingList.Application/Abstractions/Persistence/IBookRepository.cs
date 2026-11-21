using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Abstractions.Persistence;

/// <summary>
/// Persistence port for books. Every query is scoped by owner so a user can never reach another user's books.
/// </summary>
public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Book>> ListByOwnerAsync(Guid ownerId, ReadingStatus? status = null, CancellationToken cancellationToken = default);

    Task AddAsync(Book book, CancellationToken cancellationToken = default);

    /// <returns><c>true</c> when a row was updated; <c>false</c> when the book does not exist for that owner.</returns>
    Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default);

    /// <returns><c>true</c> when a row was deleted; <c>false</c> when the book does not exist for that owner.</returns>
    Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
}
