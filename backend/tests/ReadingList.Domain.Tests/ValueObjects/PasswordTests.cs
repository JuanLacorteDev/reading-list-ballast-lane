using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Domain.Tests.ValueObjects;

public sealed class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ReturnsPassword()
    {
        var password = Password.Create("Secret123!");

        Assert.Equal("Secret123!", password.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPassword_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Password.Create(value));
    }

    [Fact]
    public void Create_WithPasswordAtMinLength_Succeeds()
    {
        var value = new string('a', Password.MinLength);

        Assert.Equal(value, Password.Create(value).Value);
    }

    [Fact]
    public void Create_WithPasswordShorterThanMinLength_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Password.Create(new string('a', Password.MinLength - 1)));
    }

    [Fact]
    public void Create_WithPasswordAtMaxLength_Succeeds()
    {
        var value = new string('a', Password.MaxLength);

        Assert.Equal(value, Password.Create(value).Value);
    }

    [Fact]
    public void Create_WithPasswordLongerThanMaxLength_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Password.Create(new string('a', Password.MaxLength + 1)));
    }

    [Fact]
    public void Create_KeepsSurroundingSpaces()
    {
        var password = Password.Create(" Secret123! ");

        Assert.Equal(" Secret123! ", password.Value);
    }

    [Fact]
    public void ToString_DoesNotExposeValue()
    {
        var password = Password.Create("Secret123!");

        Assert.DoesNotContain("Secret123!", password.ToString());
    }
}
