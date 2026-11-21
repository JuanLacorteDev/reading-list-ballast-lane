using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Domain.Tests.Entities;

public sealed class UserTests
{
    private const string ValidName = "Reader";
    private const string ValidPasswordHash = "hashed-password";
    private static readonly Email ValidEmail = Email.Create("reader@example.com");

    [Fact]
    public void Create_WithValidData_ReturnsUser()
    {
        var before = DateTime.UtcNow;

        var user = User.Create(ValidEmail, ValidName, ValidPasswordHash);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(ValidEmail, user.Email);
        Assert.Equal(ValidName, user.Name);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.InRange(user.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var user = User.Create(ValidEmail, "  Reader  ", ValidPasswordHash);

        Assert.Equal("Reader", user.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => User.Create(ValidEmail, name, ValidPasswordHash));
    }

    [Fact]
    public void Create_WithNameAtMaxLength_Succeeds()
    {
        var name = new string('a', 50);

        var user = User.Create(ValidEmail, name, ValidPasswordHash);

        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void Create_WithNameLongerThanMaxLength_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.Create(ValidEmail, new string('a', 51), ValidPasswordHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPasswordHash_ThrowsDomainException(string passwordHash)
    {
        Assert.Throws<DomainException>(() => User.Create(ValidEmail, ValidName, passwordHash));
    }

    [Fact]
    public void Create_WithNullEmail_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.Create(null!, ValidName, ValidPasswordHash));
    }

    [Fact]
    public void Restore_KeepsIdAndDatesFromStorage()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2025, 1, 10, 8, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2025, 2, 20, 9, 30, 0, DateTimeKind.Utc);

        var user = User.Restore(id, ValidEmail, ValidName, ValidPasswordHash, createdAt, updatedAt);

        Assert.Equal(id, user.Id);
        Assert.Equal(createdAt, user.CreatedAt);
        Assert.Equal(updatedAt, user.UpdatedAt);
        Assert.Equal(ValidEmail, user.Email);
        Assert.Equal(ValidName, user.Name);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
    }
}
