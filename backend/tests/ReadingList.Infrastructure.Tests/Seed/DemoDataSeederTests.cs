using Microsoft.Extensions.Logging.Abstractions;
using ReadingList.Domain.ValueObjects;
using ReadingList.Infrastructure.Persistence.Books;
using ReadingList.Infrastructure.Persistence.Seed;
using ReadingList.Infrastructure.Persistence.Users;
using ReadingList.Infrastructure.Security;
using ReadingList.Infrastructure.Tests.Repositories;

namespace ReadingList.Infrastructure.Tests.Seed;

public sealed class DemoDataSeederTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _db = new();
    private UserRepository _users = null!;
    private BookRepository _books = null!;
    private DemoDataSeeder _sut = null!;
    private readonly Pbkdf2PasswordHasher _hasher = new();

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _users = new UserRepository(_db.Factory);
        _books = new BookRepository(_db.Factory);
        _sut = new DemoDataSeeder(_users, _books, _hasher, NullLogger<DemoDataSeeder>.Instance);
    }

    public Task DisposeAsync() => _db.DisposeAsync();

    [Fact]
    public async Task Seed_CreatesDemoUserWithWorkingPasswordAndBooks()
    {
        await _sut.SeedAsync();

        var user = await _users.GetByEmailAsync(Email.Create(DemoDataSeeder.DemoEmail));
        Assert.NotNull(user);
        Assert.True(_hasher.Verify(DemoDataSeeder.DemoPassword, user.PasswordHash));
        Assert.NotEmpty(await _books.ListByOwnerAsync(user.Id));
    }

    [Fact]
    public async Task Seed_RunTwice_DoesNotDuplicateData()
    {
        await _sut.SeedAsync();
        var user = await _users.GetByEmailAsync(Email.Create(DemoDataSeeder.DemoEmail));
        var booksAfterFirstRun = (await _books.ListByOwnerAsync(user!.Id)).Count;

        await _sut.SeedAsync();

        Assert.Equal(booksAfterFirstRun, (await _books.ListByOwnerAsync(user.Id)).Count);
    }
}
