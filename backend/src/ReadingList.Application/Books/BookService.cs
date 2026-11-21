using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Application.Common.Results;
using ReadingList.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Application.Books
{
    public sealed class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }
        public Task<Result<BookResponse>> CreateAsync(Guid ownerId, CreateBookRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteAsync(Guid ownerId, Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<BookResponse>> GetByIdAsync(Guid ownerId, Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IReadOnlyList<BookResponse>>> ListAsync(Guid ownerId, ReadingStatus? status, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<BookResponse>> UpdateAsync(Guid ownerId, Guid id, UpdateBookRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
