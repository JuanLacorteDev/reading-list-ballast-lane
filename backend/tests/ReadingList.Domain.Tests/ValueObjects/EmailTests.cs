using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Domain.Tests.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsEmail()
    {
        var email = Email.Create("reader@example.com");

        Assert.Equal("reader@example.com", email.Value);
    }

    [Fact]
    public void Create_NormalizesToTrimmedLowerCase()
    {
        var email = Email.Create("  Reader@Example.COM ");

        Assert.Equal("reader@example.com", email.Value);
    }

    [Fact]
    public void Create_SameAddressWithDifferentCase_AreEqual()
    {
        Assert.Equal(Email.Create("READER@example.com"), Email.Create("reader@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("reader@")]
    [InlineData("@example.com")]
    [InlineData("reader example@example.com")]
    public void Create_WithInvalidEmail_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value));
    }

    [Fact]
    public void Create_WithEmailLongerThan254Chars_ThrowsDomainException()
    {
        var value = new string('a', 245) + "@example.com";

        Assert.Throws<DomainException>(() => Email.Create(value));
    }
}
