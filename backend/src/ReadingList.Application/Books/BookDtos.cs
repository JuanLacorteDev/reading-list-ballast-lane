using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Books;

public sealed record CreateBookRequest(string Title, string Author);

public sealed record UpdateBookRequest(string Title, string Author, ReadingStatus Status, int? Rating);

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Author,
    ReadingStatus status,
    DateTime createdAt)
{
    public static BookResponse FromDomain(Book book) => new(
        book.Id,
        book.Title,
        book.Author,
        book.Status,
        book.CreatedAt);
}
