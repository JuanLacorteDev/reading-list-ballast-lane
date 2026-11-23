using TaskManager.Application.Tasks;
using TaskManager.Application.Users;
using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.UnitTests.Application;

public class ListTasksUseCaseTests
{
    private readonly TaskTestContext _ctx = new();

    [Fact]
    public async Task Execute_ReturnsOnlyCurrentUsersTasks()
    {
        var mine1 = _ctx.SeedTask(_ctx.CurrentUser, "Mine 1");
        var mine2 = _ctx.SeedTask(_ctx.CurrentUser, "Mine 2");
        _ctx.SeedTask(_ctx.OtherUser, "Theirs");

        var result = await new ListTasksUseCase(_ctx.CurrentUserContext, _ctx.Users, _ctx.Tasks).ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { mine1.Id, mine2.Id }.Order(),
            result.Value.Select(t => t.Id).Order());
    }

    [Fact]
    public async Task Execute_WithUnknownUser_ReturnsUserNotFound()
    {
        var sut = new ListTasksUseCase(new FakeCurrentUserContext(Guid.NewGuid()), _ctx.Users, _ctx.Tasks);

        var result = await sut.ExecuteAsync();

        Assert.Equal(UserErrors.NotFound, result.Error);
    }
}

public class GetTaskUseCaseTests
{
    private readonly TaskTestContext _ctx = new();
    private GetTaskUseCase Sut => new(_ctx.CurrentUserContext, _ctx.Users, _ctx.Tasks);

    [Fact]
    public async Task Execute_OwnTask_ReturnsIt()
    {
        var task = _ctx.SeedTask(_ctx.CurrentUser, "Mine");

        var result = await Sut.ExecuteAsync(task.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(task.Id, result.Value.Id);
        Assert.Equal("Mine", result.Value.Title);
    }

    [Fact]
    public async Task Execute_OtherUsersTask_ReturnsTaskNotFound()
    {
        var theirs = _ctx.SeedTask(_ctx.OtherUser);

        var result = await Sut.ExecuteAsync(theirs.Id);

        Assert.Equal(TaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_MissingTask_ReturnsTaskNotFound()
    {
        var result = await Sut.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(TaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_WithUnknownUser_ReturnsUserNotFound()
    {
        var task = _ctx.SeedTask(_ctx.CurrentUser);
        var sut = new GetTaskUseCase(new FakeCurrentUserContext(Guid.NewGuid()), _ctx.Users, _ctx.Tasks);

        var result = await sut.ExecuteAsync(task.Id);

        Assert.Equal(UserErrors.NotFound, result.Error);
    }
}

public class UpdateTaskUseCaseTests
{
    private readonly TaskTestContext _ctx = new();
    private UpdateTaskUseCase Sut => new(_ctx.CurrentUserContext, _ctx.Users, _ctx.Tasks);

    [Fact]
    public async Task Execute_OwnTask_UpdatesAllFields()
    {
        var task = _ctx.SeedTask(_ctx.CurrentUser);

        var result = await Sut.ExecuteAsync(task.Id,
            new UpdateTaskCommand("Updated", "Details", "Completed", "2027-01-15"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", result.Value.Title);
        Assert.Equal("Details", result.Value.Description);
        Assert.Equal(TaskItemStatus.Completed, result.Value.Status);
        Assert.Equal(new DateOnly(2027, 1, 15), result.Value.DueDate);
    }

    [Fact]
    public async Task Execute_OtherUsersTask_ReturnsNotFoundAndLeavesItUnchanged()
    {
        var theirs = _ctx.SeedTask(_ctx.OtherUser, "Theirs");

        var result = await Sut.ExecuteAsync(theirs.Id,
            new UpdateTaskCommand("Hijacked", null, "Completed", "2027-01-15"));

        Assert.Equal(TaskErrors.NotFound, result.Error);
        Assert.Equal("Theirs", theirs.Title);
        Assert.Equal(TaskItemStatus.Pending, theirs.Status);
    }

    [Fact]
    public async Task Execute_WithoutStatus_ReturnsValidationError()
    {
        var task = _ctx.SeedTask(_ctx.CurrentUser);

        var result = await Sut.ExecuteAsync(task.Id, new UpdateTaskCommand("Title", null, null, "2027-01-15"));

        Assert.Equal(TaskErrors.StatusRequired, result.Error);
    }

    [Fact]
    public async Task Execute_WithBlankTitle_LetsDomainExceptionPropagate()
    {
        var task = _ctx.SeedTask(_ctx.CurrentUser, "Original");

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            Sut.ExecuteAsync(task.Id, new UpdateTaskCommand(" ", null, "Completed", "2027-01-15")));

        Assert.Equal("Task.TitleRequired", ex.Code);
        Assert.Equal("Original", task.Title);
    }
}

public class DeleteTaskUseCaseTests
{
    private readonly TaskTestContext _ctx = new();
    private DeleteTaskUseCase Sut => new(_ctx.CurrentUserContext, _ctx.Users, _ctx.Tasks);

    [Fact]
    public async Task Execute_OwnTask_RemovesIt()
    {
        var task = _ctx.SeedTask(_ctx.CurrentUser);

        var result = await Sut.ExecuteAsync(task.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(_ctx.Tasks.Tasks);
    }

    [Fact]
    public async Task Execute_OtherUsersTask_ReturnsNotFoundAndKeepsIt()
    {
        var theirs = _ctx.SeedTask(_ctx.OtherUser);

        var result = await Sut.ExecuteAsync(theirs.Id);

        Assert.Equal(TaskErrors.NotFound, result.Error);
        Assert.Contains(theirs, _ctx.Tasks.Tasks);
    }

    [Fact]
    public async Task Execute_WithUnknownUser_ReturnsUserNotFound()
    {
        var sut = new DeleteTaskUseCase(new FakeCurrentUserContext(Guid.NewGuid()), _ctx.Users, _ctx.Tasks);

        var result = await sut.ExecuteAsync(Guid.NewGuid());

        Assert.Equal(UserErrors.NotFound, result.Error);
    }
}
