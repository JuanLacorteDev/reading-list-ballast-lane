using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Tests.Fakes
{
    public sealed class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books = [];

        public IReadOnlyList<Book> Books => _books;   

        public Task<Book?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
            => Task.FromResult(_books.SingleOrDefault(b => b.Id == id && b.OwnerId == ownerId));

        public Task<IReadOnlyList<Book>> ListByOwnerAsync(Guid ownerId, ReadingStatus? status = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Book>>(
                _books.Where(b => b.OwnerId == ownerId && (status is null || b.Status == status)).ToList());

        public Task AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            _books.Add(book);
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
            => Task.FromResult(_books.Any(b => b.Id == book.Id && b.OwnerId == book.OwnerId));

        public Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
            => Task.FromResult(_books.RemoveAll(b => b.Id == id && b.OwnerId == ownerId) > 0);
    }
}
