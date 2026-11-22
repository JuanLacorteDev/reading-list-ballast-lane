using Microsoft.Data.Sqlite;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;
using ReadingList.Infrastructure.Persistence;
using ReadingList.Infrastructure.Persistence.Books;
using ReadingList.Infrastructure.Persistence.Users;

namespace ReadingList.Infrastructure.Tests.Repositories;

public sealed class BookRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _db = new();
    private BookRepository _sut = null!;
    private UserRepository _users = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _sut = new BookRepository(_db.Factory);
        _users = new UserRepository(_db.Factory);
    }

    public Task DisposeAsync() => _db.DisposeAsync();

    private async Task<Guid> SeedUserAsync(string email = "reader@example.com")
    {
        var user = User.Create(Email.Create(email), "Reader", "hashed-password");
        await _users.AddAsync(user);
        return user.Id;
    }

    [Fact]
    public async Task AddThenGetById_RoundTripsAllFields()
    {
        var ownerId = await SeedUserAsync();
        var book = Book.Create(ownerId, "Clean Architecture", "Robert C. Martin");
        book.UpdateDetails("Clean Architecture", "Robert C. Martin", ReadingStatus.Read);

        await _sut.AddAsync(book);
        var loaded = await _sut.GetByIdAsync(book.Id, ownerId);

        Assert.NotNull(loaded);
        Assert.Equal(book.Id, loaded.Id);
        Assert.Equal(ownerId, loaded.OwnerId);
        Assert.Equal("Clean Architecture", loaded.Title);
        Assert.Equal("Robert C. Martin", loaded.Author);
        Assert.Equal(ReadingStatus.Read, loaded.Status);
        Assert.Equal(book.CreatedAt, loaded.CreatedAt);
    }

    [Fact]
    public async Task GetById_WithAnotherOwner_ReturnsNull()
    {
        var ownerId = await SeedUserAsync();
        var otherId = await SeedUserAsync("other@example.com");
        var book = Book.Create(ownerId, "Refactoring", "Martin Fowler");
        await _sut.AddAsync(book);

        var loaded = await _sut.GetByIdAsync(book.Id, otherId);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListByOwner_ReturnsOnlyOwnerBooks()
    {
        var ownerId = await SeedUserAsync();
        var otherId = await SeedUserAsync("other@example.com");
        await _sut.AddAsync(Book.Create(ownerId, "Book A", "Author A"));
        await _sut.AddAsync(Book.Create(ownerId, "Book B", "Author B"));
        await _sut.AddAsync(Book.Create(otherId, "Book C", "Author C"));

        var books = await _sut.ListByOwnerAsync(ownerId, status: null);

        Assert.Equal(2, books.Count);
        Assert.All(books, b => Assert.Equal(ownerId, b.OwnerId));
    }

    [Fact]
    public async Task ListByOwner_WithStatus_Filters()
    {
        var ownerId = await SeedUserAsync();

        var toRead = Book.Create(ownerId, "Book A", "Author A");
        var read = Book.Create(ownerId, "Book B", "Author B");
        read.UpdateDetails("Book B", "Author B", ReadingStatus.Read, 4);

        await _sut.AddAsync(toRead);
        await _sut.AddAsync(read);

        var books = await _sut.ListByOwnerAsync(ownerId, ReadingStatus.Read);

        var single = Assert.Single(books);
        Assert.Equal(read.Id, single.Id);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var ownerId = await SeedUserAsync();
        var book = Book.Create(ownerId, "Old title", "Old author");
        await _sut.AddAsync(book);

        book.UpdateDetails("New title", "New author", ReadingStatus.Read, 3);
        var updated = await _sut.UpdateAsync(book);
        var loaded = await _sut.GetByIdAsync(book.Id, ownerId);

        Assert.True(updated);
        Assert.NotNull(loaded);
        Assert.Equal("New title", loaded.Title);
        Assert.Equal("New author", loaded.Author);
        Assert.Equal(ReadingStatus.Read, loaded.Status);
        Assert.Equal(3, loaded.Rating);
        Assert.Equal(book.UpdatedAt, loaded.UpdatedAt);
    }

    [Fact]
    public async Task Update_WithAnotherOwner_ReturnsFalse()
    {
        var ownerId = await SeedUserAsync();
        var otherId = await SeedUserAsync("other@example.com");
        var book = Book.Create(ownerId, "Title", "Author");
        await _sut.AddAsync(book);

        // Same Id, different owner: simulates someone trying to edit a book that isn't theirs.
        var forged = Book.Restore(book.Id, otherId, "Hacked", "Hacker", ReadingStatus.WantToRead, null, book.CreatedAt, DateTime.UtcNow);
        var updated = await _sut.UpdateAsync(forged);
        var loaded = await _sut.GetByIdAsync(book.Id, ownerId);

        Assert.False(updated);
        Assert.Equal("Title", loaded!.Title);
    }

    [Fact]
    public async Task Delete_RemovesRow()
    {
        var ownerId = await SeedUserAsync();
        var book = Book.Create(ownerId, "Title", "Author");
        await _sut.AddAsync(book);

        var deleted = await _sut.DeleteAsync(book.Id, ownerId);

        Assert.True(deleted);
        Assert.Null(await _sut.GetByIdAsync(book.Id, ownerId));
    }

    [Fact]
    public async Task Delete_WithAnotherOwner_ReturnsFalseAndKeepsRow()
    {
        var ownerId = await SeedUserAsync();
        var otherId = await SeedUserAsync("other@example.com");
        var book = Book.Create(ownerId, "Title", "Author");
        await _sut.AddAsync(book);

        var deleted = await _sut.DeleteAsync(book.Id, otherId);

        Assert.False(deleted);
        Assert.NotNull(await _sut.GetByIdAsync(book.Id, ownerId));
    }

    [Fact]
    public async Task Add_WithNonExistentOwner_ViolatesForeignKey()
    {
        var book = Book.Create(Guid.NewGuid(), "Orphan", "Nobody");

        var ex = await Assert.ThrowsAsync<SqliteException>(() => _sut.AddAsync(book));

        Assert.Equal(19, ex.SqliteErrorCode);
    }
}
