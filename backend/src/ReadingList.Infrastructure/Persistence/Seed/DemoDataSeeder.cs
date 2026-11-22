using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Application.Abstractions.Security;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Infrastructure.Persistence.Seed;

/// <summary>
/// Creates a demo user with a few books so the app can be tried right away.
/// Goes through the domain and the repositories, so seeded data obeys the same rules as user data.
/// </summary>
public sealed class DemoDataSeeder
{
    public const string DemoEmail = "demo@readinglist.dev";
    public const string DemoPassword = "Demo@123";
    private const string DemoName = "Demo Reader";
    private const int SqliteConstraintError = 19;

    private readonly IUserRepository _users;
    private readonly IBookRepository _books;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(
        IUserRepository users,
        IBookRepository books,
        IPasswordHasher passwordHasher,
        ILogger<DemoDataSeeder> logger)
    {
        _users = users;
        _books = books;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var email = Email.Create(DemoEmail);
        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            return;

        var user = User.Create(email, DemoName, _passwordHasher.Hash(Password.Create(DemoPassword).Value));

        try
        {
            await _users.AddAsync(user, cancellationToken);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteConstraintError)
        {
            // Both APIs seed on startup. If the other one won the race, there is nothing left to do.
            return;
        }

        foreach (var book in DemoBooks(user.Id))
            await _books.AddAsync(book, cancellationToken);

        _logger.LogInformation("Seeded demo user {Email}.", DemoEmail);
    }

    private static IEnumerable<Book> DemoBooks(Guid ownerId)
    {
        yield return Book.Create(ownerId, "The Pragmatic Programmer", "Andrew Hunt, David Thomas");

        var cleanCode = Book.Create(ownerId, "Clean Code", "Robert C. Martin");
        cleanCode.UpdateDetails(cleanCode.Title, cleanCode.Author, ReadingStatus.Read, 4);
        yield return cleanCode;

        var ddd = Book.Create(ownerId, "Domain-Driven Design", "Eric Evans");
        ddd.ChangeStatus(ReadingStatus.Reading);
        yield return ddd;

        var refactoring = Book.Create(ownerId, "Refactoring", "Martin Fowler");
        refactoring.UpdateDetails(refactoring.Title, refactoring.Author, ReadingStatus.Read, 5);
        yield return refactoring;

        var patterns = Book.Create(ownerId, "Patterns of Enterprise Application Architecture", "Martin Fowler");
        patterns.ChangeStatus(ReadingStatus.Abandoned);
        yield return patterns;
    }
}
