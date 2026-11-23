using TaskManager.Domain.Common;
using TaskManager.Domain.Users;

namespace TaskManager.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidName_GeneratesIdAndTrimsName()
    {
        var user = User.Create("  Ada  ");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Ada", user.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? name)
    {
        Assert.Throws<DomainException>(() => User.Create(name!));
    }
}
