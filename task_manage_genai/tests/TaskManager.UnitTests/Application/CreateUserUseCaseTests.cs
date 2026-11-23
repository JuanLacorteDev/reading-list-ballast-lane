using TaskManager.Application.Users;
using TaskManager.Domain.Common;

namespace TaskManager.UnitTests.Application;

public class CreateUserUseCaseTests
{
    [Fact]
    public async Task Execute_WithValidName_PersistsUserAndReturnsId()
    {
        var users = new InMemoryUserRepository();
        var useCase = new CreateUserUseCase(users);

        var result = await useCase.ExecuteAsync(new CreateUserCommand("Ada"));

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(users.Users);
        Assert.Equal(stored.Id, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task Execute_WithBlankName_LetsDomainExceptionPropagate(string? name)
    {
        var users = new InMemoryUserRepository();
        var useCase = new CreateUserUseCase(users);

        var ex = await Assert.ThrowsAsync<DomainException>(() => useCase.ExecuteAsync(new CreateUserCommand(name)));

        Assert.Equal("User.NameRequired", ex.Code);
        Assert.Empty(users.Users);
    }
}
