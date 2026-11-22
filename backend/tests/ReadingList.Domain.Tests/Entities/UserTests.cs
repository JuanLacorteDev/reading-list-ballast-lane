using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Domain.Tests.Entities;

public sealed class UserTests
{
    private const string ValidName = "Reader";
    private const string ValidPasswordHash = "hashed-password";
    private static readonly Email ValidEmail = Email.Create("reader@example.com");

    [Fact(DisplayName = "Should create a user when valid data is provided and return the user")]
    public void Create_WithValidData_ReturnsUser()
    {
        //Arrange
        var before = DateTime.UtcNow;

        //Act
        var user = User.Create(ValidEmail, ValidName, ValidPasswordHash);


        //Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(ValidEmail, user.Email);
        Assert.Equal(ValidName, user.Name);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.InRange(user.CreatedAt, before, DateTime.UtcNow);
        Assert.Null(user.UpdatedAt);
    }

    [Fact(DisplayName = "Should create a user and remove leading and trailing from the name")]
    public void Create_TrimsName()
    {
        //Arrange and Act
        var user = User.Create(ValidEmail, "  Reader  ", ValidPasswordHash);

        //Assert
        Assert.Equal("Reader", user.Name);
    }

    [Theory(DisplayName = "Should throw a domain excpetion when name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => User.Create(ValidEmail, name, ValidPasswordHash));
    }

    [Fact(DisplayName = "Should accept name with max length (50 caracters)")]
    public void Create_WithNameAtMaxLength_Succeeds()
    {
        //Arrange
        var name = new string('a', 50);
        //Act
        var user = User.Create(ValidEmail, name, ValidPasswordHash);
        //Assert 
        Assert.Equal(name, user.Name);
    }

    [Fact(DisplayName = "Should throw domain excpetion when name over max length (50 caracters)")]
    public void Create_WithNameLongerThanMaxLength_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.Create(ValidEmail, new string('a', 51), ValidPasswordHash));
    }

    [Theory(DisplayName = "Should throw domain excpetion when password empty is provided")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPasswordHash_ThrowsDomainException(string passwordHash)
    {
        Assert.Throws<DomainException>(() => User.Create(ValidEmail, ValidName, passwordHash));
    }

    [Fact(DisplayName = "Should throw domain excpetion when password empty is provided")]
    public void Create_WithNullEmail_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.Create(null!, ValidName, ValidPasswordHash));
    }

    [Fact(DisplayName = "Should keep the provided data into restore")]
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
