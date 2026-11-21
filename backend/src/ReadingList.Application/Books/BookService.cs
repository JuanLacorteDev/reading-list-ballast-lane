using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Application.Common.Results;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Books
{
    public sealed class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }
        public async Task<Result<BookResponse>> CreateAsync(Guid ownerId, CreateBookRequest request, CancellationToken cancellationToken = default)
        {

            var book = Book.Create(ownerId, request.Title, request.Author);
            await _bookRepository.AddAsync(book, cancellationToken);

            return BookResponse.FromDomain(book);
        }

        public async Task<Result> DeleteAsync(Guid ownerId, Guid id, CancellationToken cancellationToken = default)
        {
            var deleted = await _bookRepository.DeleteAsync(id, ownerId, cancellationToken);
            return deleted ? Result.Success() : Result.Failure(BookErrors.NotFound);
        }

        public async Task<Result<BookResponse>> GetByIdAsync(Guid ownerId, Guid id, CancellationToken cancellationToken = default)
        {
            var book = await _bookRepository.GetByIdAsync(id, ownerId, cancellationToken);
            if (book is null)
                return BookErrors.NotFound;

            return BookResponse.FromDomain(book);
        }

        public async Task<Result<IReadOnlyList<BookResponse>>> ListAsync(Guid ownerId, ReadingStatus? status, CancellationToken cancellationToken = default)
        {
            var books = await _bookRepository.ListByOwnerAsync(ownerId, status, cancellationToken);
            return books.Select(BookResponse.FromDomain).ToList();
        }

        public async Task<Result<BookResponse>> UpdateAsync(Guid ownerId, Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default)
        {
            var book = await _bookRepository.GetByIdAsync(id, ownerId, cancellationToken);
            if (book is null)
                return BookErrors.NotFound;

            book.UpdateDetails(request.Title, request.Author, request.Status, request.Rating);
            var updated = await _bookRepository.UpdateAsync(book, cancellationToken);

            return updated ? BookResponse.FromDomain(book) : BookErrors.NotFound;
        }
    }
}
