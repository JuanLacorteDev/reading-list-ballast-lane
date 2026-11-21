using ReadingList.Application.Common.Results;
using ReadingList.Domain.Common.Enums;

namespace ReadingList.Application.Books;

/// <summary>
/// Book use cases. The owner id always comes from the authenticated user, never from the request body.
/// </summary>
public interface IBookService
{
    Task<Result<IReadOnlyList<BookResponse>>> ListAsync(Guid ownerId, ReadingStatus? status, CancellationToken cancellationToken = default);

    Task<Result<BookResponse>> GetByIdAsync(Guid ownerId, Guid id, CancellationToken cancellationToken = default);

    Task<Result<BookResponse>> CreateAsync(Guid ownerId, CreateBookRequest request, CancellationToken cancellationToken = default);

    Task<Result<BookResponse>> UpdateAsync(Guid ownerId, Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid ownerId, Guid id, CancellationToken cancellationToken = default);
}
